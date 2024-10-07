import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
    if not ST.ExistsGS(301) then
        ST.SetEnt("startDoor", false)
        ST.SetGS(301, 1)
    elseif ST.GetGS(301) >= 2 then
        ST.SetNPCState("mom", 0, 1)
        ST.SetNPCState("dad", 0, 1)
    end
end

function trigger_trig_dinner()
    if ST.GetGS(301) == 1 then
        ST.StartScene(2)       
    elseif ST.GetGS(301) == 2 then
        ST.SelfDialogue("self3")
    end
end

function trigger_trig_dinner1()
    if ST.GetGS(301) == 1 then
        ST.StartScene(1)       
    end
end

function trigger_trig_bedroom()
    if ST.GetGS(301) == 1 then
        ST.SetGS(301, 2)
        ST.SetEnt("startDoor", true)
        ST.Blackout()
        ST.SetNPCState("mom", 0, 1)
        ST.SetNPCState("dad", 0, 1)
        ST.StartScene(6)
    end
end