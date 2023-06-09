FREESPACE
=
[![.NET](https://github.com/DrCosinus/FreeSpace/actions/workflows/CI.yml/badge.svg)](https://github.com/DrCosinus/FreeSpace/actions/workflows/CI.yml)

Simple tool to show disk contents

Improvements to be done
-

- [x] data-time before log messages
- [x] speed up the process
- [ ] disable update button while updating...
- [ ] progress bar
- [ ] partial analysis: only analyse a folder
- [ ] show advices and potential space gain (temp folders, download folders, caches, recycle bin, windows restore points,...)
- [ ] see installed apps and games
- [ ] see in explorer
- [ ] dynamic updates (the application listens to file creations, updates, ...)
- [ ] logical drives/folders vs physical (ignore logical ones?) symbolic
- [ ] "Files as a folder"
- [ ] Automatic Timestamp and version number

> Bad ideas :
> - ~~I can retrieve all files in one pass with SearchOption.AllDirectories~~  
it will try to retrieve unauthorized folders when use from root of drives and raise exception

## How to obtain the target of a symbolic link (or Reparse Point) using .Net?
https://stackoverflow.com/questions/2302416/how-to-obtain-the-target-of-a-symbolic-link-or-reparse-point-using-net