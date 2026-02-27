#include <format>
#include <fstream>
#include <iostream>
#include <sstream>
#include <windows.h>

#include "Main.h"

//#include "SDK.hpp" // multiple assertion failures
#include "SDK/Engine_classes.hpp"

HANDLE hPipe;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID lpReserved)
{
  switch (reason)
  {
  case DLL_PROCESS_ATTACH:
    InitPipe();

    HANDLE hThread = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)MainThread, hModule, 0, 0);
    //HANDLE hThread = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)MainThreadDev, hModule, 0, 0);

    // This closes the handle to the thread object and releases the reference to the thread so you don’t leak a kernel handle.
    if (hThread) CloseHandle(hThread);
    break;
  }

  return TRUE;
}

void InitPipe()
{
  hPipe = CreateFileW(TEXT("\\\\.\\pipe\\tlcpipe"), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
}

DWORD MainThread(HMODULE aModule)
{
  HMODULE hModule = static_cast<HMODULE>(aModule);
  static bool lConsoleEnabled = false;

  // C:\Program Files (x86)\Steam\steamapps\common\Voyage\Voyage\Binaries\Win64\TLCCompanion-dll.log.txt
  std::ofstream logfile("TLCCompanion-dll.log.txt");

  std::cout << "Start of MainThread." << std::endl;
  logfile << "Start of MainThread." << std::endl;

  char lRequestBuffer[128];
  while (true)
  {
    DWORD lBytesRead = 0;

    // Blocks until a request is received
    BOOL lResult = ReadFile(hPipe, lRequestBuffer, sizeof(lRequestBuffer), &lBytesRead, NULL);

    if (!lResult || lBytesRead == 0)
    { 
      CloseHandle(hPipe); 
      hPipe = INVALID_HANDLE_VALUE;

      // Pipe broken or closed 
      InitPipe(); 
      Sleep(500);
      continue; 
    }    

    SDK::UEngine* lEngine = SDK::UEngine::GetEngine();
    if (!lEngine)
    {
      Sleep(500);
      continue;
    }

    SDK::UWorld* lWorld = SDK::UWorld::GetWorld();
    if (!lWorld || !lWorld->OwningGameInstance)
    {
      Sleep(500);
      continue;
    }

    auto lLocalPlayers = lWorld->OwningGameInstance->LocalPlayers;
    if (lLocalPlayers.Num() == 0 || !lLocalPlayers[0] || !lLocalPlayers[0]->PlayerController)
    {
      Sleep(500);
      continue;
    }

    SDK::APlayerController* lPlayerController = lLocalPlayers[0]->PlayerController;
    if (!lPlayerController || !lPlayerController->AcknowledgedPawn)
    {
      Sleep(500);
      continue;
    }

    std::string lCommand(lRequestBuffer, lBytesRead);
    auto trim = [](std::string& s) 
    { 
      while (!s.empty() && (s.back() == '\n' || s.back() == '\r' || s.back() == ' ')) s.pop_back();
    }; 
    trim(lCommand);

    if (lCommand == "CMD_GET_LOCATION")
    {
      auto lLocation = lPlayerController->AcknowledgedPawn->K2_GetActorLocation();
      
      std::ostringstream lOstringstream;
      lOstringstream
        << "{"
        << "\"x\":" << lLocation.X << ","
        << "\"y\":" << lLocation.Y << ","
        << "\"z\":" << lLocation.Z
        << "}\n";

      std::string lDataToSend = lOstringstream.str();

      lResult = WriteFile(hPipe, lDataToSend.c_str(), strlen(lDataToSend.c_str()), nullptr, NULL);
      if (!lResult)
      {
        CloseHandle(hPipe);
        hPipe = INVALID_HANDLE_VALUE;

        InitPipe(); 
        continue;
      }
    }
    else if (lCommand == "CMD_CONSOLE_ON")
    {
      if (!lConsoleEnabled)
      {
        AllocConsole();
        FILE* lDummy;
        freopen_s(&lDummy, "CONOUT$", "w", stdout);
        freopen_s(&lDummy, "CONIN$", "r", stdin);
        lConsoleEnabled = true;
      }
    }
    else if (lCommand == "CMD_CONSOLE_OFF")
    {
      if (lConsoleEnabled)
      {
        FreeConsole();
        lConsoleEnabled = false;
      }
    }
    else if (lCommand == "CMD_DISCONNECT")
    {
      CloseHandle(hPipe); 
      hPipe = INVALID_HANDLE_VALUE;

      break;
    }
    else
    {
      std::cout << "Unknown command: " << lCommand << std::endl;
      logfile << "Unknown command: " << lCommand << std::endl;
    }

    Sleep(250);
  }

  std::cout << "End of MainThread." << std::endl;
  logfile << "End of MainThread." << std::endl;

  // Self-unload 
  FreeLibraryAndExitThread(hModule, 0);
  return 0; // never reached
}

DWORD MainThreadDev(HMODULE Module)
{
  HMODULE hModule = static_cast<HMODULE>(Module);

  /* Code to open a console window */
  //AllocConsole();
  //HANDLE hOut = GetStdHandle(STD_OUTPUT_HANDLE);

  //// Step 1: shrink window so buffer can grow
  //CONSOLE_SCREEN_BUFFER_INFO csbi;
  //GetConsoleScreenBufferInfo(hOut, &csbi);

  //SMALL_RECT rect = csbi.srWindow;
  //rect.Bottom = rect.Top + 50;   // visible window height = 50 lines
  //rect.Right = rect.Left + 120; // visible window width = 120 columns
  //SetConsoleWindowInfo(hOut, TRUE, &rect);

  //// Step 2: now increase buffer safely
  //COORD bufferSize;
  //bufferSize.X = 300;
  //bufferSize.Y = 32767;
  //SetConsoleScreenBufferSize(hOut, bufferSize);

  /* Code to open a console window */
  AllocConsole();

  FILE* Dummy;
  freopen_s(&Dummy, "CONOUT$", "w", stdout);
  freopen_s(&Dummy, "CONIN$", "r", stdin);

  // Open a log file 
  std::ofstream logfile("dump_log.txt");

  // Your code here
  std::cout << "Start of MainThread." << std::endl;
  logfile << "Start of MainThread." << std::endl;

  /* Functions returning "static" instances */
  SDK::UEngine* Engine = SDK::UEngine::GetEngine();
  SDK::UWorld* World = SDK::UWorld::GetWorld();

  if (!Engine || !World || !World->OwningGameInstance ||
    World->OwningGameInstance->LocalPlayers.Num() == 0 ||
    !World->OwningGameInstance->LocalPlayers[0] ||
    !World->OwningGameInstance->LocalPlayers[0]->PlayerController)
  {
    std::cout << "One or more pointers were null, aborting." << std::endl;
    logfile << "One or more pointers were null, aborting." << std::endl;
  }
  else
  {
    /* Getting the PlayerController, World, OwningGameInstance, ... should all be checked not to be nullptr! */
    SDK::APlayerController* MyController = World->OwningGameInstance->LocalPlayers[0]->PlayerController;

    double locationX = MyController->AcknowledgedPawn->K2_GetActorLocation().X;
    double locationY = MyController->AcknowledgedPawn->K2_GetActorLocation().Y;
    double locationZ = MyController->AcknowledgedPawn->K2_GetActorLocation().Z;

    std::cout << "locationX: " << locationX << ", locationY: " << locationY << ", locationZ: " << locationZ << std::endl;

    /* Print the full-name of an object ("ClassName PackageName.OptionalOuter.ObjectName") */
    std::cout << Engine->ConsoleClass->GetFullName() << std::endl;
    logfile << Engine->ConsoleClass->GetFullName() << std::endl;

    std::cout << "GObjects count: " << SDK::UObject::GObjects->Num() << std::endl;
    logfile << "GObjects count: " << SDK::UObject::GObjects->Num() << std::endl;

    /* Manually iterating GObjects and printing the FullName of every UObject that is a Pawn (not recommended) */
    for (int i = 0; i < SDK::UObject::GObjects->Num(); i++)
    {
      SDK::UObject* Obj = SDK::UObject::GObjects->GetByIndex(i);

      if (!Obj)
        continue;

      if (Obj->IsDefaultObject())
        continue;

      continue;

      std::string lFullName = Obj->GetFullName();
      std::cout << lFullName << "\n";
      logfile << lFullName << "\n";

      /* Only the 'IsA' check using the cast flags is required, the other 'IsA' is redundant */
      //if (Obj->IsA(SDK::APawn::StaticClass()) || Obj->HasTypeFlag(SDK::EClassCastFlags::Pawn))
      //{
      //  std::cout << "---" << lFullName << "\n";
      //  logfile << "---" << lFullName << "\n";
      //}
      ////else if (Obj->GetFullName().starts_with("BP"))
      //else if (lFullName.find("PersistentLevel") != std::string::npos)
      //{
      //  std::cout << lFullName << "\n";
      //  logfile << lFullName << "\n";
      //}
    }
  }

  std::cout << "End of MainThread." << std::endl;
  logfile << "End of MainThread." << std::endl;

  // Optional: close console 
  // FreeConsole();

  // Self-unload 
  CloseHandle(hPipe);
  FreeLibraryAndExitThread(hModule, 0);
  return 0; // never reached
}