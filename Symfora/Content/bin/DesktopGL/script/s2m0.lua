import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

--local music = 0;

function init()
end

function trigger_trig_tutorial()
    ST.Tutorial(0)
    ST.Tutorial(1)
    ST.Tutorial(2)
    ST.Tutorial(3)
    ST.Tutorial(4)
    --ST.Tutorial(5)
    ST.Tutorial(6)
end

function trigger_trig_tutorial1()
    ST.Tutorial(7)
end

function trigger_trig_tutorial2()
    ST.Tutorial(8)
    ST.SetEnt("trig_tutorial4", false)
    ST.SetEnt("trig_tutorial", false)
    ST.SetEnt("switch3", false)
end

function trigger_trig_tutorial4()
    ST.Tutorial(13)
    --music = ST.PlayMusic("testMusic", 1)
end

function switch_switch3()
    ST.FlipEnt("door5")
end

function switch_switch1(active)
    ST.SetNPCState("chase", 1, 1)
    ST.FlipEnt("door3")
end

function switch_switch2(active)
    ST.FlipEnt("door1")
    ST.FlipEnt("door2")
end

function magnet_magnet1(holding)
    ST.FlipEnt("door4")
end

function npc_madCitizen()
    ST.Tutorial(9)
    ST.Tutorial(10)
end

function trigger_trig_chase()
    ST.FlipEnt("door5")
    ST.FlipEnt("chaseClip")
    ST.SetNPCState("chase", 0, 1)
end