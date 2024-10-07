import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function switch_switch1(active)
    ST.FlipEnt("door1")
end

function magnet_magnet1(holding)
    ST.FlipEnt("door2")
end

function magnet_magnet2(holding)
    ST.FlipEnt("door6")
end

function switch_switch2(active)
    ST.FlipEnt("door3")
    ST.FlipEnt("door4")
end

function switch_switch3(active)
    ST.FlipEnt("door5")
    ST.FlipEnt("door7")
end

function trigger_trig_doorBlock(active)
    ST.FlipEnt("door5")
end

function trigger_trig_boss(active)
    ST.FlipEnt("door4")
    ST.AwakeBoss("boss")
end