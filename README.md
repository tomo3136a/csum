# csum

S-Record check sum calculator

## Usage

### normal calculate

command:

    > csum.exe test.srec

result:

    # 2023/02/05 18:10:17
    file name   : test.srec
    file size   : 236
    note        : hello
    record count: 3
    entry point : 0000
    byte count  : 70
    sum         : 5641
    sum (hex)   : 00001609

    # 00:00:00.0352506

### ROM size specification

Fill undefined areas with 0xff.
The ROM size can be specified with the command option '-size'.
The ROM size is specified in 'MB'.

Example of ROM size of 2MB

command:

    csum.exe -size=2 test.srec

result:

    # 2023/02/05 18:11:15
    file name   : test.srec
    file size   : 236
    note        : hello
    record count: 3
    entry point : 0000
    byte count  : 70
    rom size    : 2MB (fill=0xFF)
    sum         : 534761551
    sum (hex)   : 1FDFD04F

    # 00:00:00.0312806

## Build

    > Build.cmd

result `csum.exe`.

## Note

Overwrite not supported
