import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function death()
    if ST.GetGS(301) == 5 then
        if ST.GetGS(67) == 1 then
            ST.SetGS(301, 4)
        else
            ST.SetGS(301, 3)
        end
    end
end

function trigger_trig_khaled()
    ST.StartScene(5)
end

function switch_tome(active)
    ST.SetGS(67 + 7, 1)
end

function trigger_trig_spy()
    ST.StartScene(7)
end
function trigger_trig_encounter()
    ST.StartScene(8)
    ST.SetGS(301, 5)
end

function scene8_act8()
    ST.SetNPCState("gorjan", 0, 1)
end

function trigger_trig_gorjan()
    if ST.GetGS(301) == 5 then
        ST.SetNPCState("gorjan", 1, 1)
    end
end