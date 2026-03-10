# ConsoleApp

Simple C# console project scaffolded manually in this repository.

## 1) Download portable .NET SDK (on your PC)

```bash
python tools/download_dotnet.py --channel 8.0 --install-dir third_party/dotnet
```

## 2) Use local SDK and run

```bash
export DOTNET_ROOT="$PWD/third_party/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
dotnet run --project ConsoleApp/ConsoleApp.csproj
```
