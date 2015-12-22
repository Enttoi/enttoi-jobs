# Enttoi API for web client

[![Build status](https://ci.appveyor.com/api/projects/status/mi0xgwxrpo7kburj/branch/master?svg=true)](https://ci.appveyor.com/project/jenyayel/enttoi-api-dotnet/branch/master)

*This repository is part of [Enttoi](http://enttoi.github.io/) project.*

This is a server side component for web interface. This is an OWIN based application that exposes WebApi for gettings sensors and clients, and [SignalR](https://github.com/SignalR/SignalR) for pushing real-time updates on sensors state.

## Running in dev

1. Compile using from VS 
2. Configure environment variables located in Startup.cs
3. WebApi endpoints:

  ```
  /clients/all
  /clients/{client id}
  ```
  This will return a medat data of clients and their sensors

4. To get initial state and updates of sensors:
  * Connect to SignalR at ```/signalr```
  * Create proxy ```CommonHub```
  * Subscribe to event ```SensorStatePush```
  * Execute ```RequestInitialState``` method

