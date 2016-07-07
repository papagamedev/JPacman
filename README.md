#JPacman

![jpacman_icon.png](https://bitbucket.org/repo/6GMoL8/images/3192814779-jpacman_icon.png)

JPacman is a classic _Pac-man_ clone that runs on PC with Windows. It's a personal project I coded in 1998-1999 as a gift to my mom, a super fan of the original Pacman game.

It has the same Pacman game mechanics, but I added a couple of nice extra features. 

In late 2015 I found the original code in an old backup disc and I decided to revive the project and, eventually, port it to other platforms. Just for fun.

The original name of the game comes from my name (Juan Pablo) and Pacman: _JP_ + _Pacman_ = _JPacman_

## Repository Contents

### JPacman1999, the original version

#### The original C++ source code

In the _JPacman1999_ folder you can find the original version of the source, as coded back in 1999. It was written using MS Visual C++ 6.0, if I recall, but I've adapted it to work with MS Visual Studio Community 2015.

#### The music of the original game

The game features some music that I wrote myself using the old (and deprecated) DirectMusic Composer.

The short intro/ghosts/gameover short tunes are _not_ original, but a set of tracks trying to mimic what I recalled from the original game.

The main ingame music is also _not_ original, but my own cover of the old New Rally X game tune (Namco, 1981), which I love by the way.

#### The original game binaries

You can also find some binaries I got from those years, which are still running in all Windows versions I've tested.

There are two subfolders there:

- _win7-_: to run the game in Windows 7 or older versions of Windows. Confirmed to work on Windows 98 SE, Windows XP and Windows 7.

- _win8+_: to run the game in Windows 8 or newer versions of Windows. Confirmed to work on on Windows 8, Windows 8.1 and Windows 10.

#### Important notice for user on Windwos 8 or newer versions

As mentioned above, in Windows 8 or newer versions, you need to run the Binaries in the "Win8+" folder, given that the game needs a DirectDraw wrapper to run smoothly. This is caused because Microsoft decided to drop support for Hardware acceleration on DirectDraw in Windwos 8, breaking a lot of old games like mine.

Many people have coded some wrappers to fix the issue. The only one I found that works is the one coded by "Aqrit" (http://bitpatch.com/ddwrapper.html). For conveniency, I've included the "Aqrit's ddwrapper" in the "Win8+" folder, along with its readme file.

Please make sure to check his website (http://bitpatch.com) and check his other cool tools.