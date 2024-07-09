# BetterPrefs

BetterPrefs is both a replacement for Unity's PlayerPrefs with features that PlayerPrefs is lacking, such as support for multiple saves, save import/export and even more data types, such as booleans, Vector2s and Vector3s, but is designed to, while keeping the simplicity of PlayerPrefs, be perfect for game data.

It is a great, versatile, universal system for any kind of save data, be it progress, settings, creations, you name it.

BetterPrefs is designed to be similar to PlayerPrefs, so switching over is super easy, barely an inconvenience.

License: [MIT](https://opensource.org/licenses/MIT)

## Features

- **Multiple saves**: Save data can be saved to multiple files, and the user can switch between them.
- **Serialization**: Saves are serialized, which makes them harder to edit (PlayerPrefs usually stores them in plain-text or in registry, making them super easy to edit and to cheat) ([more info](#serialization))
- **Save import/export**: Save data can be imported and exported, which is very useful for sharing save data between different devices.
- **Cross-platform**: Save data can be saved to a file on the desktop, and loaded from a file on Android, etc. All platforms Unity supports can read the files in the same way.
- **Data types**: BetterPrefs supports more data types than Unity's PlayerPrefs, such as booleans, Vector2s and Vector3s.
- **Data info**: Unlike PlayerPrefs, BetterPrefs lets you check how many keys you've stored, or get all your save data as a `Dictionary<string,object>`.
- **Open source**: The source code is public, unlike Unity's PlayerPrefs.
- **More to come**: More features are coming soon, like importing old PlayerPrefs saves and converting them to BetterPrefs

## Getting Started

To start using BetterPrefs, you need to add the [BetterPrefs.cs](https://github.com/Carroted/BetterPrefs/blob/master/BetterPrefs.cs) script to your Unity project.

Once you've added the script, you can use the BetterPrefs class to access your save data. BetterPrefs is not a singleton, and, in fact, does not derive from MonoBehaviour at all. It is instead a static class that you can access from anywhere, without even needing any `using`s for it.

```csharp
BetterPrefs.Load("/saves/example.save"); // Loads the save data from "/saves/example.save", or, if it doesn't exist, loads a blank save and remembers to save at that location

BetterPrefs.SetBool("My Bool", true);
BetterPrefs.SetInt("My Int", 5);
BetterPrefs.SetFloat("My Float", 5.5f);
BetterPrefs.SetString("My String", "Hello World!\nNewline supported!");
BetterPrefs.SetVector2("My Vector2", new Vector2(1.25f, 5.7f));
BetterPrefs.SetVector3("My Vector3", new Vector3(1.25f, 5.7f, 3.5f));

BetterPrefs.Save(); // Saves the data to the current save file, in this case "/saves/example.save"
BetterPrefs.Save("/saves/backups/game.save"); // Saves a backup of the data to the file "/saves/backups/game.save"
```

Unlike PlayerPrefs, BetterPrefs lets you easily choose which save file to load and save to.

In the above example, we even create a backup of the data, so that if something goes wrong, we can still load the data we saved before.

BetterPrefs doesn't automatically save the data on quit due to Unity limitations (scripts that don't derive from MonoBehaviour can't access Unity's [OnApplicationQuit](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html) event), but this can actually be helpful, as you can create your own dialog that asks if you want to save or not.

You are recommended to create a "save manager", which can take many forms. You could have a UI on game start which asks which save you want to load from a few slots (if people share devices), you could automatically load a save from a certain location, etc. This save manager could run `BetterPrefs.Save` in [OnApplicationQuit](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html).

If you want to automatically load a specific save when you enter the game, calling `BetterPrefs.Load` on [Awake](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html) is a great way to start.

## Migration from PlayerPrefs

If your Unity project currently uses PlayerPrefs, switching should be easy.

The instructions in [Getting Started](#getting-started) should be helpful.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleSaveLoader : MonoBehaviour
{
    void Awake()
    {
        BetterPrefs.Load(); // You can specify a path if you want, or you could make a UI where you can choose your save
    }
}
```

Then, you can use the BetterPrefs class to access your save data.

If you do want to have a UI to manage saves in your game, you should make sure your code doesn't try to access data before the BetterPrefs.Load method has been called. A great way to do this is to make that UI be in its own scene.

You can get a list of saves by listing files in the saves directory, which, by default, is `Application.persistentDataPath + "/saves"`, and then add a list of save options the user can choose from, and call BetterPrefs.Load on the chosen save.

Once your save manager is done and ready, you might be able to simply do a find and replace on your scripts from `PlayerPrefs` to `BetterPrefs`. (can be done easily in most IDEs). **MAKE SURE TO HAVE A BACKUP BEFORE DOING THIS!**

It's also worth noting that old PlayerPrefs saves currently aren't supported, and if many people already have your game and have saves in it, those saves will be lost. This should only be an issue before release.

## Reference

### Methods

- `void` **BetterPrefs.Load()**: Loads the save data from the default save file.
- `void` **BetterPrefs.Load(string path)**: Loads the save data from the specified file.
- `string` **BetterPrefs.Save()**: Saves the save data into the save that was Loaded and returns the path where it saved.
- `string` **BetterPrefs.Save(string path)**: Saves the data to the specified file and returns the path where it saved.
- `void` **BetterPrefs.SetBool(string key, bool value)**: Sets the value of the specified key to the specified boolean value.
- `void` **BetterPrefs.SetInt(string key, int value)**: Sets the value of the specified key to the specified integer value.
- `void` **BetterPrefs.SetFloat(string key, float value)**: Sets the value of the specified key to the specified float value.
- `void` **BetterPrefs.SetString(string key, string value)**: Sets the value of the specified key to the specified string value.
- `void` **BetterPrefs.SetVector2(string key, Vector2 value)**: Sets the value of the specified key to the specified Vector2 value.
- `void` **BetterPrefs.SetVector3(string key, Vector3 value)**: Sets the value of the specified key to the specified Vector3 value.
- `bool` **BetterPrefs.GetBool(string key)**: Gets the value of the specified key as a boolean.
- `int` **BetterPrefs.GetInt(string key)**: Gets the value of the specified key as an integer.
- `float` **BetterPrefs.GetFloat(string key)**: Gets the value of the specified key as a float.
- `string` **BetterPrefs.GetString(string key)**: Gets the value of the specified key as a string.
- `Vector2` **BetterPrefs.GetVector2(string key)**: Gets the value of the specified key as a Vector2.
- `Vector3` **BetterPrefs.GetVector3(string key)**: Gets the value of the specified key as a Vector3.

- `bool` **BetterPrefs.GetBool(string key, bool fallback)**: Gets the value of the specified key as a boolean, or the fallback if the key doesn't exist.
- `int` **BetterPrefs.GetInt(string key, int fallback)**: Gets the value of the specified key as an integer, or the fallback if the key doesn't exist.
- `float` **BetterPrefs.GetFloat(string key, float fallback)**: Gets the value of the specified key as a float, or the fallback if the key doesn't exist.
- `string` **BetterPrefs.GetString(string key, string fallback)**: Gets the value of the specified key as a string, or the fallback if the key doesn't exist.
- `Vector2` **BetterPrefs.GetVector2(string key, Vector2 fallback)**: Gets the value of the specified key as a Vector2, or the fallback if the key doesn't exist.
- `Vector3` **BetterPrefs.GetVector3(string key, Vector3 fallback)**: Gets the value of the specified key as a Vector3, or the fallback if the key doesn't exist.

- `void` **BetterPrefs.DeleteKey(string key)**: Deletes the specified key.
- `void` **BetterPrefs.DeleteAll()**: Deletes all keys.
- `DateTime` **BetterPrefs.GetDate()**: Gets the `DateTime` of the currently loaded save file, or the current date if the save hasn't been Saved yet.
- `DateTime` **BetterPrefs.GetDate(string path)**: Gets the `DateTime` of when the specified save file was saved.
- `bool` **BetterPrefs.HasKey(string key)**: Checks if the specified key exists.
- `int` **BetterPrefs.GetCount()**: Get how many keys there are in the data
- `Dictionary<string,object>` **BetterPrefs.GetData()**: Get the currently loaded save data

### Variables
- `string` **saveLocation**: Where saves are stored by default (overriden with a path argument to `Load` and `Save`)
- `string` **saveExtension**: Default file extension for saves (overriden with a path argument to `Load` and `Save`)
- `string` **currentSave**: Path to the currently loaded save, or null if none is loaded.

There is also a `Dictionary<string,object>` named **data** in BetterPrefs, but it should only ever be modified with the above methods.

## Serialization

Saves are serialized using `System.IO.BinaryWriter` using [BetterPrefs.cs](https://github.com/Carroted/BetterPrefs/blob/master/BetterPrefs.cs)'s `Write2DArray` (credit to GitHub Copilot for that)

## Known Limitations

- PlayerPrefs saves can't be imported

## About

BetterPrefs was made to fix many of PlayerPrefs' limitations, and to make a save/load system that can be used for more than just player preferences.

It was released on May 21, 2022 by [amytimed](https://github.com/amytimed) under the [MIT license](https://opensource.org/licenses/MIT), as it will stay forever.
