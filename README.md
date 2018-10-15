# WSL git.exe proxy

This project provides a `git.exe` that interoperates between Windows and WSL.
It does path conversions, etc.

Because of a [bug in WSL](https://github.com/Microsoft/WSL/issues/3246), some users' mounted drives don't get recognized properly by WSL.
This breaks path conversions and the working directory when calling `wsl.exe`.
The file `wslpath` is a python script intended to work around that. 

This proxy converts the paths of arguments and maintains ANSI colors.

## Setup

- Place `wslpath` at `/home/user/bin/wslpath` in the WSL filesystem. You can adjust the path in Program.cs.
- Compile this project and put the executeable somewhere in your PATH. It serves as a drop-in replacement for `git.exe`.
