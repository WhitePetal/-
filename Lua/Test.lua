local DFA = require('DFA')

DFA:InitFilterCfg('filterword.xml')
DFA:InitFilter()
local str = DFA:SearchFilterWordAndReplace("哈哈哈我操你妈哈哈哈")
print(str)
io.read()