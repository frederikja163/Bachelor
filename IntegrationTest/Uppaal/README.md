# Integration Tests

## Test 1
command: `.\TimedRegex.exe "(((AB)[0;1]C)&(A(BC)[1;20]))"`

result: [integrationtest1.xml](integrationtest1.xml)

## Test 2
command: `.\TimedRegex.exe "A[1;3]|B[4;5]C[3;6]"`

result: [integrationtest2.xml](integrationtest2.xml)

## Test 3
command: `.\TimedRegex.exe "A&(A[1;5]B{BA})"`

result: [integrationtest3.xml](integrationtest3.xml)

## Test 4
command: `.\TimedRegex.exe "(A[1;5])+'"`

result: [integrationtest4.xml](integrationtest4.xml)