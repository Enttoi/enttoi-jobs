# Enttoi scheduled and continuous jobs

[![Build status](https://ci.appveyor.com/api/projects/status/mi0xgwxrpo7kburj/branch/master?svg=true)](https://ci.appveyor.com/project/jenyayel/enttoi-jobs/branch/master)

The set of jobs that executed either on scheduled basis or in always-running mode. They are implemented as [Azure Web Jobs](https://azure.microsoft.com/en-us/documentation/articles/websites-webjobs-resources/).

#### ClientsState
Reads client's latest ping time from blob storage and updates the state of the client in meta data store of clients.

