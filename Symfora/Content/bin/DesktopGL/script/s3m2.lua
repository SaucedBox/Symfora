import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function magnet_magnet1(holding)
    ST.FlipEnt("door1")
end

function switch_bal1_switch(state)
    if state then
        ST.ShootBallista("ballista1")
    end
end
function switch_bal2_switch(state)
    if state then
        ST.ShootBallista("ballista2")
        cd = ST.GetEnt("clogDoor1")
        if not cd.Active then
            ST.SetEnt("door4", false)
            ST.SetEnt("door9", false)
            ST.SetEnt("bal2_switch", false)
        end
    end
end
function switch_bal3_switch(state)
    if state then
        ST.ShootBallista("ballista3")
        cd = ST.GetEnt("clogDoor2")
        if not cd.Active then
            ST.SetEnt("door8", false)
            ST.SetEnt("bal3_switch", false)
        end
    end
end
function switch_bal4_switch(state)
    if state then
        ST.ShootBallista("ballista4")
        cd = ST.GetEnt("clogDoor3")
        if not cd.Active then
            ST.SetEnt("door5", false)
            ST.SetNPCState("clog", 0, 3)
            ST.SetEnt("bal4_switch", false)
        end
    end
end

function magnet_magnet2(holding)
    ST.FlipEnt("door9")
    ST.FlipEnt("clogDoor1")
end

function magnet_magnet3(holding)
    ST.FlipEnt("clogDoor2")
end

function magnet_magnet4(holding)
    ST.FlipEnt("door6")
end

function magnet_magnet6(holding)
    ST.FlipEnt("door10")
end

function timer_timer1(state)
    ST.SetEnt("door7", not state)
end

function magnet_magnet5(holding)
    ST.FlipEnt("clogDoor3")
end

function npc_clog()
    ST.FlipEnt("clogClip")
end

function switch_tome(active)
    ST.SetGS(67 + 2, 1)
end