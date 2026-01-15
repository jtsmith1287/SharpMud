using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using GameCore;
using GameCore.Util;
using ServerCore.Util;

namespace ServerCore {
public class Connection : IDisposable {
    private static readonly object BigLock = new object();
    private static readonly ArrayList Connections = new ArrayList();

    public readonly StreamReader Reader;

    private readonly Socket _socket;
    private readonly StreamWriter _writer;

    private PlayerEntity _player;

    public Connection(Socket socket) {
        this._socket = socket;
        Reader = new StreamReader(new NetworkStream(socket, false));
        _writer = new StreamWriter(new NetworkStream(socket, true));
        new Thread(ClientLoop).Start();
    }

    void ClientLoop() {
        try {
            OnConnect();

            if (_player == null) {
                return;
            }

            while (_socket.Connected) {
                lock (BigLock) {
                    foreach (Connection conn in Connections) {
                        if (conn._writer != null) {
                            conn._writer.Flush();
                        }
                    }
                }

                string line = Reader.ReadLine();
                if (line == null) {
                    break;
                }

                lock (BigLock) {
                    if (_player != null) {
                        ArgumentHandler.HandleLine(line.Trim(), _player);
                    }
                }
            }
        } catch (Exception e) {
            Console.WriteLine("Error in ClientLoop: " + e.Message);
        } finally {
            OnDisconnect();
        }
    }

    public void Send(string msg) {
        try {
            if (_writer != null) {
                _writer.WriteLine(msg);
                _writer.Flush();
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

            if (Data.UsernamePwdPairs.TryGetValue(username, out truePwd)) {
                Send("Password:\n");
                // Wait for user to input password
                providedPwd = Reader.ReadLine();
                if (providedPwd == null) {
                    return;
                }

                if (providedPwd == truePwd) {
                    if (Data.UsernameIdPairs.TryGetValue(username, out var id)) {
                        if (Data.IdDataPairs.TryGetValue(id, out var data)) {
                            _player = new PlayerEntity(this, data);
                            break;
                        } else {
                            Send("Error: User data not found. Please contact an admin.");
                            this._socket.Close();
                            return;
                        }
                    } else {
                        Send("Error: User ID not found. Please contact an admin.");
                        this._socket.Close();
                        return;
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
                        _player = new PlayerEntity(this, new Data(username, Guid.NewGuid()));

                        Data.UsernamePwdPairs.Add(username, providedPwd);
                        Data.UsernameIdPairs.Add(username, _player.ID);

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
                    Data.SaveData(DataPaths.UserId, DataPaths.UserPwd, DataPaths.IdData);
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
            Console.WriteLine(_player.ID + " has disconnected");
        } else {
            Console.WriteLine("An unauthenticated connection has disconnected");
        }

        Dispose();
    }

    public void Dispose() {
        if (_player != null) {
            Console.Write("Disposing of " + _player.Name + "'s resources...");
        } else {
            Console.Write("Disposing of unauthenticated connection's resources...");
        }

        if (Reader != null) {
            Reader.Dispose();
        }

        try {
            if (_writer != null) {
                _writer.Dispose();
            }
        } catch (IOException) {
            if (_player != null) {
                Console.WriteLine(_player.Name + " might not have been fully disposed of.");
            }
        }

        Console.WriteLine(" done!");
    }
}
}
