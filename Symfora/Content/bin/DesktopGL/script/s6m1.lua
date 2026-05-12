import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function switch_switch1(state)
    ST.FlipEnt("door1")
end
function switch_switch2(state)
    ST.FlipEnt("door2")
end
function switch_switch3(state)
    ST.FlipEnt("catalyst1")
end

function magnet_magnet2(holding)
    ST.FlipEnt("door3")
    ST.FlipEnt("door4")
end
function magnet_magnet1(holding)
    ST.FlipEnt("door5")
end

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function trigger_trig_blockBoosts()
    ST.BlockSelfBoosts(true)
    if not ST.GetEnt("door4").Active and ST.GetEnt("switch3").Switched then
        ST.SetEnt("door4", true)
    end
end
function trigger_trig_letBoosts()
    ST.BlockSelfBoosts(false)
end

function trigger_trig_boss()
    ST.StartScene(12)
    ST.AwakeBoss("boss")
end
function npc_jesersant()
    ST.SelfDialogue("self13")
end

function switch_tome(active)
    ST.SetGS(67 + 5, 1)
end

function custom_jesersant(phase)
    ST.SetEnt("bossSwitch" .. tostring(phase), true)
end

function switch_bossSwitch1(state)
    ST.SetNPCState("boss", 3, 1)
end
function switch_bossSwitch2(state)
    ST.SetNPCState("boss", 3, 2)
end
function switch_bossSwitch3(state)
    ST.SetNPCState("boss", 3, 4)
end

function npc_boss()
    ST.SetEnt("door6", false);
end