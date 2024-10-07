import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

local facDoorBlocked = false

function trigger_trig_doorBlock()
    facDoorBlocked = true
    ST.FlipEnt("door7")
end
function trigger_trig_doorBlock1()
    ST.FlipEnt("door3")
end
function magnet_magnet1(holding)
    ST.FlipEnt("door1")
end
function switch_switch1(state)
    ST.FlipEnt("door3")
end
function switch_switch2(state)
    ST.FlipEnt("door4")
end
function switch_switch3(state)
    ST.FlipEnt("door5")
end

function timer_timer1(state)
    ST.SetEnt("switch2", not state)

    door = ST.GetEnt("door4")
    if door.Active and state then
        ST.SetEnt("door4", false)
    elseif not door.Active and not state then
        ST.SetEnt("door4", true)
    end
end

local tridinp1 = 0
function magnet_magnet2(holding)
    if not facDoorBlocked then
        triDoor1(holding)
    end
end
function magnet_magnet3(holding)
    if not facDoorBlocked then
        triDoor1(holding)
    end
end
function magnet_magnet4(holding)
    if not facDoorBlocked then
        triDoor1(holding)
    end
end

function triDoor1(state)
    if state then
        tridinp1 = tridinp1 + 1
    else
        tridinp1 = tridinp1 - 1
    end
    tridinp1 = ST.IntClamp(tridinp1, 0, 3)

    door = ST.GetEnt("door7")
    if tridinp1 == 3 then
        if door.Active then
            ST.SetEnt("door7", false)
        end
    elseif not door.Active then
        ST.SetEnt("door7", true)
    end
end

local tridinp2 = 0
function magnet_magnet5(holding)
    triDoor2(holding)
end
function magnet_magnet6(holding)
    triDoor2(holding)
end
function magnet_magnet7(holding)
    triDoor2(holding)
end

function triDoor2(state)
    if state then
        tridinp2 = tridinp2 + 1
    else
        tridinp2 = tridinp2 - 1
    end
    tridinp2 = ST.IntClamp(tridinp2, 0, 3)

    door = ST.GetEnt("door6")
    if tridinp2 == 3 then
        if door.Active then
            ST.SetEnt("door6", false)
        end
    elseif not door.Active then
        ST.SetEnt("door6", true)
    end
end