# csum

check sum calculator

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

### iput file format

file format is supported S-Record, IHex and Binary.
default S-Record.
format selecting used command line argument.

- `-type=ihex` IHex format
- `-type=binary` Binary format

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

### S0 record encoding

The S0 record encoding specified with the command option '-enc'.
Default is ASCII.

Example encoding name is 'utf-8', 'sjis', 'euc-jp', ...

command:

    csum.exe -enc=utf-8 test.srec

### output result file

result to output file by command line argument.
output value is `sum`, `count` and `size`.
output file format is binary.

```
csum.exe -out=sum,count test.srec
```

#### specification

exsample `sum`.
same `count` and `size`.

output file size:

- `sum` is 8 byte size
- `sum:1` is 1 byte size
- `sum:2` is 2 byte size
- `sum:4` is 4 byte size

output value is 1st-, 2nd- .

- `sum1` is 1st-
- `sum2` is 2nd-

output byte order:

- `sum` is little
- `sum:-` is big

## Build

    > Build.cmd

result `csum.exe`.

## Note

Overwrite not supported
