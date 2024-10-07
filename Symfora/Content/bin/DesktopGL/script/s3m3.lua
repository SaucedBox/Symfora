import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function trigger_trig_blockBoosts()
    ST.BlockSelfBoosts(true)
end
function trigger_trig_letBoosts()
    ST.BlockSelfBoosts(false)
end
function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function magnet_magnet1(holding)
    oMag = ST.GetEnt("magnet2")
    if oMag.Holding then
        if holding then
            ST.SetEnt("door1", false)
        else
            ST.SetEnt("door1", true)
        end
    end
end

function magnet_magnet2(holding)
    oMag = ST.GetEnt("magnet1")
    if oMag.Holding then
        if holding then
            ST.SetEnt("door1", false)
        else
            ST.SetEnt("door1", true)
        end
    end
end

function magnet_magnet3(holding)
    ST.FlipEnt("door2")
end

function magnet_magnet4(holding)
    ST.FlipEnt("door3")
end

function switch_switch1(holding)
    ST.SetEnt("door6", false)
end

function switch_switch2(holding)
    ST.SetEnt("door7", false)
end

function trigger_trig_bossStart(holding)
    ST.SetEnt("door6", true)
end