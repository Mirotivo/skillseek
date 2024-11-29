# Getting Started with MAUI on Windows:

## 1. Install JDK

Download and install JDK 17 from the official Oracle website:
[Download JDK 17 for Windows](https://www.oracle.com/java/technologies/downloads/#jdk17-windows)

Set the following environment variables:
- `JAVA_HOME`: `C:\Program Files\Java\jdk-17`

Add the following paths to your system's `PATH`:
- `%JAVA_HOME%\bin`


## 2. Get Android Command Line Tools

Download Android command line tools from the official Android Developers website:
[Download Android Command Line Tools](https://developer.android.com/studio)

Open Command Prompt and run:
```shell
sdkmanager "cmdline-tools;latest" --sdk_root="C:\Program Files\Android\sdk"
```

Set the following environment variables:
- `ANDROID_HOME`: `C:\Program Files\Android\sdk`

Add the following paths to your system's `PATH`:
- `%ANDROID_HOME%\cmdline-tools\latest\bin`

## 3. Download Required SDK Components

Use `sdkmanager` to download SDK components:

```bash
sdkmanager --list
sdkmanager "platform-tools"
sdkmanager "platforms;android-34"
sdkmanager "build-tools;34.0.0"
sdkmanager "system-images;android-34;google_apis_playstore;x86_64"
sdkmanager --licenses
```

Add the following paths to your system's `PATH`:
- `%ANDROID_HOME%\emulator`

Create an Android Virtual Device named "pixel" using Android 34:
```bash
avdmanager list avd
avdmanager create avd -n pixel -k "system-images;android-34;google_apis_playstore;x86_64" -d "pixel"
emulator -avd pixel
```

## 4 Download MAUI

1. Install the .NET MAUI workload:

```bash
dotnet workload list
dotnet workload install maui
```

2. Create a new .NET MAUI project named Frontend.MAUI:

```bash
dotnet new maui -n Frontend.MAUI
cd Frontend.MAUI
```
3. Edit the .csproj file to set the <AndroidSdkDirectory> to your Android SDK location

```xml
<PropertyGroup>
    <AndroidSdkDirectory>$(ANDROID_HOME)</AndroidSdkDirectory>
</PropertyGroup>
```

## Building and Running

```bash
dotnet build -t:Run -f net8.0-android
dotnet build -t:Run -f net8.0-android --target pixel
```