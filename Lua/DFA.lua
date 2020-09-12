require('LuaXML')

local DFA = {
    map = {}
}

function DFA:InitFilterCfg(path)
    local xfile = xml.load(path)
    local root = xfile:find("root")
    for i = 1, #root, 1 do
        local item = root[i]
        local id = item.ID
        table.insert(self.map, item:find('word')[1])
        -- for key, value in pairs(item) do
        --     print('key: ' .. key)
        --     print('value: ', value)
        --     if key == "word" then
        --         table.insert(self.map, value)
        --     end
        -- end
    end
end

function DFA:InitFilter()
    local list = self.map
    for i = 1, #list, 1 do
        local word = list[i]
        local indexMap = self.map
        for j = 1, #word, 1 do
            local c = string.sub(word, j, j)
            if indexMap[c] then
                indexMap = indexMap[c]
            else
                local newMap = {}
                newMap["IsEnd"] = 0
                indexMap[c] = newMap
                indexMap = newMap
            end

            if j == #word then
                indexMap["IsEnd"] = 1
            end
        end
    end
end

function DFA:CheckFilterWord(txt, beginIndex)
    local flag = false
    local len = 0
    local curMap = self.map
    for i = beginIndex, #txt, 1 do
        local c = string.sub(txt, i, i)
        local temp = curMap[c]
        if temp then
            if temp["IsEnd"] == 1 then
                flag = true
            else
                curMap = temp
            end

            len = len + 1
            
        else
            break
        end
    end

    if not flag then
        len = 0
    end

    return len;
end

function DFA:SearchFilterWordAndReplace(txt)
    local i = 1
    local sb = {}
    for k = 1, #txt, 1 do
        sb[k] = string.sub(txt, k, k)
    end

    while i <= #txt do
        local len = self:CheckFilterWord(txt, i)
        if len > 0 then
            for j = 0, len, 1 do
                sb[i + j] = '*'
            end
            i = i + len
        else
            i = i + 1
        end
    end
    return table.concat(sb)
end

return DFA