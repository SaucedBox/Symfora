import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function switch_switch1(active)
    ST.FlipEnt("door1")
    ST.FlipEnt("ladder1")
end

function magnet_magnet1(holding)
    oMag = ST.GetEnt("magnet2")
    if oMag.Holding then
        if holding then
            ST.SetEnt("door6", false)
        else
            ST.SetEnt("door6", true)
        end
    end
end

function magnet_magnet2(holding)
    oMag = ST.GetEnt("magnet1")
    if oMag.Holding then
        if holding then
            ST.SetEnt("door6", false)
        else
            ST.SetEnt("door6", true)
        end
    end
end

function magnet_magnet3(holding)
    ST.FlipEnt("door5")
end

function magnet_magnet4(holding)
    ST.FlipEnt("door4")
end

function magnet_magnet7(holding)
    ST.SetAllEnts("door2", not holding)
end

function magnet_magnet6(holding)
    oMag = ST.GetEnt("magnet5")
    if oMag.Holding then
        if holding then
            ST.SetEnt("door3", false)
        else
            ST.SetEnt("door3", true)
        end
    end
end

function magnet_magnet5(holding)
    oMag = ST.GetEnt("magnet6")
    if oMag.Holding then
        if holding then
            ST.SetEnt("door3", false)
        else
            ST.SetEnt("door3", true)
        end
    end
end