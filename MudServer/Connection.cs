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
    static object BigLock = new object();
    internal Socket socket;
    public StreamReader Reader;
    public StreamWriter Writer;
    static ArrayList connections = new ArrayList();
    public PlayerEntity Player;

    public Connection(Socket socket) {
        this.socket = socket;
        Reader = new StreamReader(new NetworkStream(socket, false));
        Writer = new StreamWriter(new NetworkStream(socket, true));
        new Thread(ClientLoop).Start();
    }

    void ClientLoop() {
        try {
            OnConnect();

            while (socket.Connected) {
                lock (BigLock) {
                    foreach (Connection conn in connections) {
                        if (conn.Writer != null) {
                            conn.Writer.Flush();
                        }
                    }
                }

                string line = Reader.ReadLine();
                if (line == null) {
                    break;
                }

                lock (BigLock) {
                    if (Player != null) {
                        ArgumentHandler.HandleLine(line.Trim(), Player);
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
            if (Writer != null) {
                Writer.WriteLine(msg);
                Writer.Flush();
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
                    if (Data.UsernameIDPairs.TryGetValue(username, out Guid id)) {
                        if (Data.IDDataPairs.TryGetValue(id, out Data data)) {
                            Player = new PlayerEntity(this, data);
                            break;
                        } else {
                            Send("Error: User data not found. Please contact an admin.");
                            return;
                        }
                    } else {
                        Send("Error: User ID not found. Please contact an admin.");
                        return;
                    }
                } else {
                    Send("Incorrect password. Sorry.");
                    this.socket.Close();
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
                        Player = new PlayerEntity(this, new Data(username, Guid.NewGuid()));

                        Data.UsernamePwdPairs.Add(username, providedPwd);
                        Data.UsernameIDPairs.Add(username, Player.ID);

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
            connections.Add(this);
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
            connections.Remove(this);
        }

        if (socket.Connected) {
            socket.Close();
        }

        if (Player != null) {
            Player.Close();
            Console.WriteLine(Player.ID + " has disconnected");
        } else {
            Console.WriteLine("An unauthenticated connection has disconnected");
        }

        Dispose();
    }

    public void Dispose() {
        if (Player != null) {
            Console.Write("Disposing of " + Player.Name + "'s resources...");
        } else {
            Console.Write("Disposing of unauthenticated connection's resources...");
        }

        if (Reader != null) {
            Reader.Dispose();
        }

        try {
            if (Writer != null) {
                Writer.Dispose();
            }
        } catch (IOException) {
            if (Player != null) {
                Console.WriteLine(Player.Name + " might not have been fully disposed of.");
            }
        }

        Console.WriteLine(" done!");
    }
}
}
