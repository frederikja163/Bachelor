# Integration Tests
The XML files linked below do not entirely correspond to the output of our program, since these XML files are formatted to look readable when opened in Uppaal. Furthermore, they use Uppaal notation for location ids (`id0`), which is different from our notation (`l0`). 

It is, however, still possible to check for many things between the output of running the commands for each test, and the expected result:

1. \# of locations
2. \# of transitions
3. Initial state
4. Final States
5. Transition sources and targets
6. Labels
   1. Intervals in guards
   2. Symbol in synchronisation
   3. Clock reset in assignment

## Test 1
command: `.\TimedRegex.exe "(((AB)[0;1]C)&(A(BC)[1;20]))"`

expected: [integrationtest1.xml](integrationtest1.xml)

## Test 2
command: `.\TimedRegex.exe "A[1;3]|B[4;5]C[3;6]"`

expected: [integrationtest2.xml](integrationtest2.xml)

## Test 3
command: `.\TimedRegex.exe "A&(A[1;5]B{BA})"`

expected: [integrationtest3.xml](integrationtest3.xml)

## Test 4
command: `.\TimedRegex.exe "(A[1;5])+'"`

expected: [integrationtest4.xml](integrationtest4.xml)