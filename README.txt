If you don't see any content when you open the CHM file, you may need to unblock it. To do this, extract the CHM file from the archive. Then, right-click it in File Explorer and select Properties. In the Properties dialog, on the General tab, tick the Unblock checkbox.

If this does not help, ensure that the path to the extracted file does not contain:

- the ".chm" string (except for the file extension),
- the '#' (hash) symbol,
- Unicode characters,
- GB 18030 characters.

These are the limitations of the default CHM Viewer application installed with Windows. These limitations may not be applicable if you use an alternate tool.
