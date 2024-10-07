import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function magnet_magnet1(holding)
    ST.FlipEnt("door1")
end
function magnet_magnet4(holding)
    ST.FlipEnt("door3")
end
function magnet_magnet2(holding)
    ST.FlipEnt("timer1")
end

function timer_timer1(state)
    triDoor("priTimer", state)
    ST.FlipEnt("jet1")
end
function magnet_magnet3(holding)
    triDoor("magnet", holding)
end
function timer_timer2(state)
    triDoor("secTimer", state)
end

local tridinp = 0
function triDoor(name, state)
    if not state then
        tridinp = tridinp - 1
    else
        tridinp = tridinp + 1
    end
    tridinp = ST.IntClamp(tridinp, 0, 3)

    door = ST.GetEnt("door4")
    if tridinp > 1 then
        if door.Active then
            ST.SetEnt("door4", false)
        end
    elseif not door.Active then
        ST.SetEnt("door4", true)
    end
end

function switch_bal1_switch(state)
    if state then
        ST.ShootBallista("ballista1")
        ST.ChangeNPCHealth("chloravoth", -1)
    end
end
function switch_switch1(state)
    ST.FlipEnt("door6")
end

function timer_timer3(state)
    door = ST.GetEnt("door6")
    if not door.Active and state then
        ST.SetEnt("door6", true)
    end
    ST.SetEnt("ballista1", state)
end

function npc_chloravoth()
    ST.FlipEnt("door5")
end

function switch_switchBoss(state)
    ST.FlipEnt("door7")
end

function trigger_trig_boss(state)
    ST.FlipEnt("door7")
    ST.AwakeBoss("boss")
end

function npc_boss()
    ST.SetEnt("door8", false)
end

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function switch_tome(active)
    ST.SetGS(67 + 8, 1)
end
function switch_tome1(active)
    ST.SetGS(67 + 3, 1)
end