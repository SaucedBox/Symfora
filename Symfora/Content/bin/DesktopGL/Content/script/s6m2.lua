import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function trigger_trig_blockBoosts()
    ST.BlockSelfBoosts(true)
end
function trigger_trig_letBoosts()
    ST.BlockSelfBoosts(false)
end

function magnet_magnet1(holding)
    ST.SetAllEnts("door1", not holding)
end
function magnet_magnet2(holding)
    ST.SetAllEnts("door2", not holding)
end
function switch_switch1(state)
    ST.SetAllEnts("door4", not state)
end

function magnet_sealMag1(state)
    other = ST.GetEnt("sealMag2")
    if other.Holding and state then
        ST.SetEnt("sealDoor", false)
    elseif not state and not ST.GetEnt("sealDoor").Active then
        ST.SetEnt("sealDoor", true)
    end
end
function magnet_sealMag2(state)
    other = ST.GetEnt("sealMag1")
    if other.Holding and state then
        ST.SetEnt("sealDoor", false)
    elseif not state and not ST.GetEnt("sealDoor").Active then
        ST.SetEnt("sealDoor", true)
    end
end