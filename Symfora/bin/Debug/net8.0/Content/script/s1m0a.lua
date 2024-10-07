import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
    ST.SetNPCState("sapiens", 0, 1)
    if ST.ExistsGS(301) then
        state = ST.GetGS(301)
        if state >= 2 then
            ST.SetNPCState("sapiens", 0, 0)
            ST.SetEnt("door3", false)
            ST.SetEnt("door2", false)
            if state >= 3 then
                ST.SetEnt("door4", false)
            end
            if state == 5 then 
                ST.SetEnt("door1", false)
            end
        end
    end
end

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function trigger_trig_foundTome()
    if ST.GetGS(301) == 4 then
        ST.StartScene(4)
    end
end

function trigger_trig_library()
    if ST.GetGS(301) == 2 then
        ST.StartScene(3)
        ST.SetEnt("door4", false)
        ST.SetEnt("trig_postLibrary", true)
        ST.SetGS(301, 3)
    end
end

function switch_tome(active)
    ST.SetGS(67, 1)
    if ST.GetGS(301) == 3 then
        ST.SetGS(301, 4)
        ST.SetEnt("trig_foundTome", true)
    end
end

function trigger_trig_postLibrary()
    if ST.GetGS(301) == 3 then
        ST.SelfDialogue("self4")
        ST.SelfDialogue("self5")
    end
end

function trigger_trig_escape()
    if ST.GetGS(301) == 5 then
        ST.SetEnt("door4", true)
        ST.SelfDialogue("self6")
        ST.SelfDialogue("self7")
    end
end