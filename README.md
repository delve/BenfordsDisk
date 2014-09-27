BenfordsDisk
============

Looking for Benford's Law in the file sizes on disk drives

Results
-------
Seems pretty close to the expected distribution. Some sample outputs below.

Performance Analysis
--------------------
Also tested the performance difference between accumulating file sizes into a List<> and accumulating straight into the final count array. It looks like there's some threshold (which may depend more on memory config than any hard value) where the time difference between the two implementations explodes. 

For 166124 files the difference was 3 milliseconds.
For 249220 files the difference was 31036 milliseconds. 

Doubling the file count raised the time by 4 orders of magnitude. This may also be due to underlying filesystem differences though mechanically the two drives should be similar enough not to impact the count. The larger file count is on an SSD while the smaller count is on spinning disk with a higher total capacity which I would expect to exacerbate timing differences more than the SSD would. It should also be noted that the larger file count is my primary OS drive which could have usage fluctuations sufficient to impact the times more or less randomly.

Fully examining the performance difference is outside the scope of this project (at least until I get curious enough about it to override other motivations... and have access to a pair of mechanically matching drives)

Partial sample output:
----------------------
    Found 166124 files.
    Digit results for array based function:
    1 -> 45971 = 27.67% of total
    2 -> 32302 = 19.44% of total
    3 -> 24694 = 14.86% of total
    4 -> 13472 = 8.11% of total
    5 -> 11674 = 7.03% of total
    6 -> 10556 = 6.35% of total
    7 -> 11967 = 7.2% of total
    8 -> 8004 = 4.82% of total
    9 -> 7484 = 4.51% of total
    Directories searched in 5416 milliseconds

Known Issues
------------
When executed against my C: drive the List method counts 3 more files than the array method. This represents a difference of something like 0.00001%. Given that relative size I'm going to leave it alone for now. I have more to learn about C# before I worry about this level of detail about filesystem operations.
