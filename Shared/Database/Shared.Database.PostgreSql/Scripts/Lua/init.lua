-- ========== Init ==========

-- Create space with format
box.schema.create_space('messages', {
    if_not_exists = true,
    format = {
        { name = 'id', type = 'integer' },
        { name = 'from', type = 'string' },
        { name = 'to', type = 'string' },
        { name = 'from_to_hash', type = 'integer' },
        { name = 'sending_time', type = 'datetime' },
        { name = 'text', type = 'string' }
    }
})

-- Drop space
-- box.schema.space.drop(512)

-- Create sequence
box.schema.sequence.create('message_id', { 
    start = 0,
    min = 0
})

-- Drop sequence
-- box.sequence.message_id:drop()

-- Create primary index with autoincrement
box.space.messages:create_index('primary', {
    type = 'TREE',
    unique = true,
    parts = { 'id' },
    sequence = 'message_id'
})

-- Create spatial index
box.space.messages:create_index('spatial', {
    type = 'TREE',
    unique = false,
    parts = { 'from_to_hash' }
})

-- Drop indexes
-- box.space.messages.index.primary:drop()
-- box.space.messages.index.spatial:drop()

-- Insert data
datetime = require("datetime")
box.space.messages:insert({
    nil,
    '11111111-1111-1111-1111-111111111111',
    '22222222-2222-2222-2222-222222222222',
    -1232226626,
    datetime.parse('2023-10-01 12:00:00'),
    'Text'
})
box.space.messages:insert({
    nil,
    '22222222-2222-2222-2222-222222222222',
    '11111111-1111-1111-1111-111111111111',
    -1232226626,
    datetime.parse('2023-10-01 12:00:00'),
    'Text'
})

-- Select datas
box.space.messages.index.primary:select({ })

-- ========== Functions ==========

-- ========== Function message_create ==========

-- Create function "message_create"
lua_code = [[
function(from, to, from_to_hash, sending_time, text)
    datetime = require("datetime")
    local result = box.space.messages:insert({ 
        nil,
        from,
        to,
        from_to_hash,
        datetime.parse(sending_time),
        text
    })
    if result then
        local id = result['id']
        return box.tuple.new({ id })
    else
        return box.tuple.new({ -1 })
    end
end
]]

box.schema.func.create('message_create', {
    language = 'LUA',
    body = lua_code
})

-- Drop function
-- box.func.message_create:drop()

-- Call function
-- box.func.message_create:call({'1', '2', 2, '2023-10-01 12:00:00', 'tescjks'})

-- ========== Function message_get_list_latest ==========

-- Create function "message_get_list_latest"
lua_code = [[
function(from_to_hash, limit)
    local records = box.space.messages.index.spatial:select({ from_to_hash }, {
        iterator = 'EQ'
    })
    if records then
        table.sort(records, function(a, b) return a['id'] > b['id'] end)
        local result = {}
        local i = 1
        for _, value in pairs(records) do
            result[i] = box.tuple.new({
                records[i]['id'],
                records[i]['from'],
                records[i]['to'],
                records[i]['from_to_hash'],
                records[i]['sending_time']:format(),
                records[i]['text']
            })
            i = i + 1
            if i == limit then
                break
            end
        end
        if result then
            return result
        else
            return {}
        end
    else
        return {}
    end
end
]]

box.schema.func.create('message_get_list_latest', {
    language = 'LUA',
    body = lua_code
})

-- Drop function
-- box.func.message_get_list_latest:drop()

-- Call function
-- box.func.message_get_list_latest:call({ -1232226626, 2 })

-- ========== Function message_get_list_newest ==========

-- Create function "message_get_list_newest"
lua_code = [[
function(from_to_hash, newest, limit)
    local records = box.space.messages.index.primary:select({ newest }, {
        iterator = 'GT'
    })
    if records then
        table.sort(records, function(a, b) return a['id'] > b['id'] end)
        local result = {}
        local i = 1
        limit = limit + 1
        for _, value in pairs(records) do
            if value['from_to_hash'] == from_to_hash then
                result[i] = box.tuple.new({
                    value['id'],
                    value['from'],
                    value['to'],
                    value['from_to_hash'],
                    value['sending_time']:format(),
                    value['text']
                })
                i = i + 1
                if i == limit then
                    break
                end
            end
        end
        if result then
            return result
        else
            return -1
        end
    else
        return -1
    end
end
]]

box.schema.func.create('message_get_list_newest', {
    language = 'LUA',
    body = lua_code
})

-- Drop function
-- box.func.message_get_list_newest:drop()

-- Call function
-- box.func.message_get_list_newest:call({ -1232226626, 5, 2 })

-- ========== Function message_get_list_oldest ==========

-- Create function "message_get_list_oldest"
lua_code = [[
function(from_to_hash, oldest, limit)
    local records = box.space.messages.index.primary:select({ oldest }, {
        iterator = 'LT'
    })
    if records then
        table.sort(records, function(a, b) return a['id'] > b['id'] end)
        local result = {}
        local i = 1
        limit = limit + 1
        for _, value in pairs(records) do
            if value['from_to_hash'] == from_to_hash then
                result[i] = box.tuple.new({
                    value['id'],
                    value['from'],
                    value['to'],
                    value['from_to_hash'],
                    value['sending_time']:format(),
                    value['text']
                })
                i = i + 1
                if i == limit then
                    break
                end
            end
        end
        if result then
            return result
        else
            return -1
        end
    else
        return -1
    end
end
]]

box.schema.func.create('message_get_list_oldest', {
    language = 'LUA',
    body = lua_code
})

-- Drop function
-- box.func.message_get_list_oldest:drop()

-- Call function
-- box.func.message_get_list_oldest:call({ -1232226626, 6, 2 })

-- ========== Function message_get_list_in_range ==========

-- Create function "message_get_list_in_range"
lua_code = [[
function(from_to_hash, newest, oldest, limit)
    local records = box.space.messages.index.primary:select({ newest }, {
        iterator = 'LT'
    })
    if records then
        table.sort(records, function(a, b) return a['id'] > b['id'] end)
        local result = {}
        local i = 1
        limit = limit + 1
        for _, value in pairs(records) do
            if value['id'] == oldest then
                break
            end
            if (value['from_to_hash'] == from_to_hash) then
                result[i] = box.tuple.new({
                    value['id'],
                    value['from'],
                    value['to'],
                    value['from_to_hash'],
                    value['sending_time']:format(),
                    value['text']
                })
                i = i + 1
                if i == limit then
                    break
                end
            end
        end
        if result then
            return result
        else
            return -1
        end
    else
        return -1
    end
end
]]

box.schema.func.create('message_get_list_in_range', {
    language = 'LUA',
    body = lua_code
})

-- Drop function
-- box.func.message_get_list_in_range:drop()

-- Call function
-- box.func.message_get_list_in_range:call({ -1232226626, 6, 2, 2 })