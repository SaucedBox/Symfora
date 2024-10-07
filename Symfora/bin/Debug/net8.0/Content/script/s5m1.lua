import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function magnet_magnet1h1(holding)
    oMag = ST.GetEnt("magnet1h2")
    if oMag.Holding then
        ST.SetEnt("door1", not holding)
    end
end
function magnet_magnet1h2(holding)
    oMag = ST.GetEnt("magnet1h1")
    if oMag.Holding then
        ST.SetEnt("door1", not holding)
    end
end

function magnet_magnet2(holding)
    ST.FlipEnt("door2")
end

function switch_switch_md1(state)
    oSwc = ST.GetEnt("switch_md2")
    if oSwc.Switched then
        ST.SetEnt("door1", false)
    end
end
function switch_switch_md2(state)
    oSwc = ST.GetEnt("switch_md1")
    if oSwc.Switched then
        ST.SetEnt("door1", false)
    end
end

function switch_switch1(state)
    ST.FlipEnt("door5")
end
function timer_timer1(state)
    ST.FlipEnt("door4")
end

local lastCode = "s"
--dogshit ahead
function switch_resetSwitch(state)
    if state then
        lastCode = "s"
        button = ST.GetEnt("codeSwitch1")
        button.Switched = false
        ST.SetEnt("codeSwitch1", true)
        button = ST.GetEnt("codeSwitch2")
        button.Switched = false
        ST.SetEnt("codeSwitch2", true)
        button = ST.GetEnt("codeSwitch3")
        button.Switched = false
        ST.SetEnt("codeSwitch3", true)
        button = ST.GetEnt("codeSwitch4")
        button.Switched = false
        ST.SetEnt("codeSwitch4", true)
    end
end

function switch_codeSwitch1(state)
    if state then
        codeSwitch("a")
        ST.SetEnt("codeSwitch1", false)
    end
end
function switch_codeSwitch2(state)
    if state then
        codeSwitch("b")
        ST.SetEnt("codeSwitch2", false)
    end
end
function switch_codeSwitch3(state)
    if state then
        codeSwitch("c")
        ST.SetEnt("codeSwitch3", false)
    end
end
function switch_codeSwitch4(state)
    if state then
        codeSwitch("d")
        ST.SetEnt("codeSwitch4", false)
    end
end

function codeSwitch(id)
    lastCode = lastCode .. id
    finalDoor = ST.GetEnt("door6")
    if lastCode == "sabcd" and finalDoor.Active then
        ST.SetEnt("door6", false)
    end
end

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end
function trigger_trig_dia()
    ST.SelfDialogue("self12")
end