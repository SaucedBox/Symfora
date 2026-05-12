import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
    ST.SetAllEnts("bossBarrier", false)
end

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end
function trigger_trig_fallPlain2()
    ST.FallKillPlayer()
end
function trigger_trig_ground()
    ST.SetAllEnts("bossBarrier", true)
    ST.SetEnt("door3", true)
    ST.SetEnt("trig_fallPlain1", false)
end
function trigger_trig_boss()
    ST.SetEnt("door1", true)
    ST.StartScene(13)
end
function trigger_trig_feed()
    ST.SetGS(67 + 6, 1)
end
function trigger_trig_blockBoosts()
    ST.BlockSelfBoosts(true)
end
function trigger_trig_dia()
    ST.SelfDialogue("self14")
    ST.SelfDialogue("self15")
    ST.SelfDialogue("self16")
end

function scene13_act7()
    ST.SetEnt("door2", true)
    ST.AwakeBoss("bafixi")
end

function scene14_act3()
    ST.SetNPCState("bafixi", 0, 7)
    ST.SetNPCState("bafixi", 1, 0)
end