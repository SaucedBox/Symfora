import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
    prog = ST.GetProgress()
    name = "doorShallows"

    if prog == 2 then
        name = "doorRelayer"
    elseif prog == 3 then
        name = "doorParalevo"
    elseif prog == 4 then
        name = "doorGuardians"
    elseif prog == 5 then
        name = "doorUpChlorals"
    elseif prog == 6 then
        name = "doorLowChlorals"
    end

    if prog >= 1 then
        ent = ST.GetEnt(name)
        ent:Deactivate()
        ST.PlaySound("soanoOpen", ent.Position.X, ent.Position.Y, 0.4, 1)
    end
end

function trigger_trig_flight()
    ST.SoanoFlight(true)
    ST.SetNPCState("npcStalker", 0, 1)
end

function trigger_trig_deFlight()
    ST.SoanoFlight(false)
    ST.SetNPCState("npcStalker", 0, 0)
end