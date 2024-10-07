import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function magnet_magnet1(holding)
    ST.FlipEnt("door1")
end

function trigger_trig_blockBoosts()
    ST.BlockSelfBoosts(true)
end
function trigger_trig_letBoosts()
    ST.BlockSelfBoosts(false)
end

function timer_timer1(state)
    ST.SetEnt("door2", not state)
end

function timer_timer2(state)
    ST.SetEnt("door2", not state)
end

function magnet_magnet2(holding)
    ST.FlipEnt("door3")
end

function magnet_magnet3(holding)
    ST.FlipEnt("door4")
end