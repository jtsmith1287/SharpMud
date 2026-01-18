using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using MudServer.Entity;
using MudServer.Util;
using MudServer.Actions;
using MudServer.Enums;

namespace MudServer.Server {
public class Connection : IDisposable {
    private static readonly object BigLock = new object();
    private static readonly ArrayList Connections = new ArrayList();

    public readonly StreamReader Reader;

    private readonly Socket _socket;
    private readonly StreamWriter _writer;
    private readonly object _writeLock = new object();

    private PlayerCharacter _player;

    public Connection() { } // For testing

    public Connection(Socket socket) {
        if (socket == null) return;
        this._socket = socket;
        Reader = new StreamReader(new NetworkStream(socket, false));
        _writer = new StreamWriter(new NetworkStream(socket, true));
        new Thread(ClientLoop).Start();
    }

    void ClientLoop() {
        string identifier = "Unauthenticated connection";
        string lastLine = null;
        try {
            OnConnect();

            if (_player == null) {
                return;
            }

            identifier = $"{_player.Name} ({_player.Id})";

            while (_socket.Connected) {
                lastLine = Reader.ReadLine();
                if (lastLine == null) {
                    break;
                }

                lock (BigLock) {
                    if (_player != null) {
                        Actions.Actions.DoAction(_player, lastLine.Trim());
                    }
                }
            }
        } catch (IOException ioe) {
            Console.WriteLine($"[INFO] Connection lost for {identifier}: {ioe.Message}");
        } catch (ObjectDisposedException) {
            Console.WriteLine($"[INFO] Connection closed for {identifier}.");
        } catch (Exception e) {
            Console.WriteLine($"[ERROR] Unexpected error in ClientLoop for {identifier}: {e.Message}");
            if (lastLine != null) {
                Console.WriteLine($"Last input received: '{lastLine}'");
            }
            Console.WriteLine(e.StackTrace);
        } finally {
            OnDisconnect();
        }
    }

    public virtual void Send(string msg) {
        try {
            lock (_writeLock) {
                if (_writer != null) {
                    _writer.WriteLine(msg);
                    _writer.Flush();
                }
            }
        } catch (Exception) { }
    }

    void OnConnect() {
        string username;
        string providedPwd;
        string truePwd;

        // Get user login information and create a new account if one doesn't exist.
        while (true) {
            Send("Please log in.\nIf we don't locate your account we'll create a new one\nUsername:\n");
            // Wait for user to input username
            username = Reader.ReadLine();
            if (username == null) {
                return;
            }

            username = username.Trim();

            if (username == string.Empty) {
                continue;
            }

            if (DataManager.UsernamePwdPairs.TryGetValue(username, out truePwd)) {
                Send("Password:\n");
                // Wait for user to input password
                providedPwd = Reader.ReadLine();
                if (providedPwd == null) {
                    return;
                }

                if (providedPwd == truePwd) {
                    if (DataManager.UsernameIdPairs.TryGetValue(username, out var id)) {
                        if (DataManager.IdDataPairs.TryGetValue(id, out var data)) {
                            _player = new PlayerCharacter(this, data);
                            break;
                        } else {
                            Send("Warning: User data not found. Recreating basic stats.");
                            data = new Stats(username, id);
                            _player = new PlayerCharacter(this, data);
                            break;
                        }
                    } else {
                        Send("Warning: User ID not found. Recreating account link.");
                        var newId = Guid.NewGuid();
                        DataManager.UsernameIdPairs.Add(username, newId);
                        var data = new Stats(username, newId);
                        _player = new PlayerCharacter(this, data);
                        break;
                    }
                } else {
                    Send("Incorrect password. Sorry.");
                    this._socket.Close();
                    return;
                }
                // Create a new user account because we didn't find one.
            } else {
                string pwdVerify;
                Send("You must be new. We've got your name now, so how about a password?\nType carefully.");
                while (true) {
                    providedPwd = Reader.ReadLine();
                    if (providedPwd == null) {
                        return;
                    }

                    Send("Verify that please...\n:");
                    pwdVerify = Reader.ReadLine();
                    if (pwdVerify == null) {
                        return;
                    }

                    if (providedPwd == pwdVerify) {
                        Send("Got it! We're entering you into the system now.");
                        _player = new PlayerCharacter(this, new Stats(username, Guid.NewGuid()));

                        DataManager.UsernamePwdPairs.Add(username, providedPwd);
                        DataManager.UsernameIdPairs.Add(username, _player.Id);

                        Send("Alright, " + username + ". You're good to go!");

                        break;
                    } else {
                        Send("I uh... Hmm. These don't match. Try that again. Be a little more careful this time.");
                        continue;
                    }
                }

                // At this point we've either loaded, or created a new user, so exit the loop.
                break;
            }
        }

        lock (BigLock) {
            Connections.Add(this);
        }

        Console.WriteLine("Starting save thread for new data");
        new Thread(
            () => {
                lock (BigLock) {
                    DataManager.SaveData(DataPaths.UserId, DataPaths.UserPwd, DataPaths.IdData);
                }
            }
        ).Start();
    }

    internal void OnDisconnect() {
        lock (BigLock) {
            Connections.Remove(this);
        }

        if (_socket.Connected) {
            _socket.Close();
        }

        if (_player != null) {
            _player.Close();
            Console.WriteLine($"{_player.Name} ({_player.Id}) has disconnected.");
        } else {
            Console.WriteLine("An unauthenticated connection has disconnected.");
        }

        Dispose();
    }

    public void Dispose() {
        string name = _player != null ? _player.Name : "unauthenticated connection";
        Console.Write($"Disposing of {name}'s resources...");

        if (Reader != null) {
            Reader.Dispose();
        }

        lock (_writeLock) {
            try {
                if (_writer != null) {
                    _writer.Dispose();
                }
            } catch (IOException) {
                Console.WriteLine($"\nWarning: {name}'s writer might not have been fully disposed of.");
            }
        }

        Console.WriteLine(" done!");
    }
}
}
