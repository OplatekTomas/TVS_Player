# TVS-Player (deprecated)
## Current version: 0.5.1 Alpha
## [Download](https://github.com/Kaharonus/TVS-Player/releases)

#### TVS-Player is Windows only desktop app written in C# & WPF that maintains your library of TV Shows.

### Development paused. Working on different project (TVSPlayer Server/Client implementation)

### Key features
- Keeping your library organised and updated
- Renaming all files to a unified format (ShowName - SxxExx - EpisodeName)
- Downloading most episodes using torrent
- Autodownload for week old episodes using torrent
- Getting info about TV shows and Episodes (Actors, release dates etc.)
- Built-in player with DXVA 2.0 support (K-Lite Codecs required for any kind of playback)
- Subtitle downloading and playback
- Torrent streaming (K-Lite Codecs required for any kind of playback, kinda buggy)

### Future features
- High quality, high speed background encoding to either lower quality or x265 for data saving
- FTP or some other service for sharing episodes between devices
- Desktop client (just FTP client)
- Mobile client
- More stuff...

### Recommended hardware
- Any recent (2009 and newer) at least dual core CPU with hyperthreading
- 3 GB of RAM
- Intel HD 530 and faster for ok-ish performance. Dedicated GPU is recommended for slightly better visuals.
- A lot of HDD space for tv shows & around 1-10 MB per TV show for cached images and data

### Installation process
You can either download installation (which will take care of everything for you), download standalone version or download and compile from source. If you want to compile from source make sure you also have K-Lite Codec Pack installed

### Changing audio language
Right now this can't be done in app for various reasons, but if you right click LAV Splitter (while playing video this icon should be available if it's not open LAV Splitter using Start menu and make sure Enable System Tray Icon is enabeled) in system tray you can change it.

### Bug fixing
If TVS-Player crashed during database import phase try to delete "C:\Users\Public\Documents\TVS-Player" and import/create again. If it crashed yet again try to download source code and compile/debug it yourself. Sorry for that.

### Screenshots

![Imgur](https://i.imgur.com/fdPnbNc.png)

![Imgur](https://i.imgur.com/9cKLppQ.png)

### Dislcaimer
I'm not responsible for your actions - since it's only fourth alpha build expect errors
Downloading and sharing TV shows might not be legal in wherever you live - check your law - I'm not responsible for your actions.


