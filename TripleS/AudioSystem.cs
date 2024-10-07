using System;
using Microsoft.Xna.Framework;
using FMOD;
using FMOD.Studio;
using System.Collections.Generic;
using System.Linq;

namespace TripleS {
    public static class AudioSystem {

        private static FMOD.System FMODSystem;
        private static FMOD.Studio.System StudioSystem;
        private static Bank mainBank;
        private static Bank masterBank;
        private static Bank stringsBank;
        private static string oldBankName;
        private static bool canPlaySound;
        public static List<ReverbZone> SpacialReverbs { get; set; }
        public static Dictionary<int, EventSoundInfo> EventInfo { get; set; }
        public static Dictionary<int, EventInstance> Instances { get; set; }

        public static void Initialize()
        {
            //temp fix
            FMODSystem = new FMOD.System();
            FMODSystem.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)0);

            EventInfo = new Dictionary<int, EventSoundInfo>();
            Instances = new Dictionary<int, EventInstance>();
            FMOD.Studio.System.create(out StudioSystem);
            StudioSystem.initialize(32, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, (IntPtr)0);
            StudioSystem.getCoreSystem(out FMODSystem);
            FMODSystem.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)0);

            StudioSystem.setNumListeners(1);
            StudioSystem.setListenerWeight(0, 1f);

            REVERB_PROPERTIES prop = PRESET.CAVE();
            FMODSystem.setReverbProperties(0, ref prop);
            prop = PRESET.ARENA();
            FMODSystem.setReverbProperties(1, ref prop);
            prop = PRESET.LIVINGROOM();
            FMODSystem.setReverbProperties(2, ref prop);
            prop = PRESET.QUARRY();
            FMODSystem.setReverbProperties(3, ref prop);
        }

        public static void Load(string bankName, string masterBankName = "Master")
        {
            if (bankName != oldBankName || mainBank.Equals(default(Bank)))
            {
                StudioSystem.unloadAll();
                StudioSystem.loadBankFile($"Content/audio/{masterBankName}.bank", LOAD_BANK_FLAGS.NORMAL, out masterBank);
                StudioSystem.loadBankFile($"Content/audio/{masterBankName}.strings.bank", LOAD_BANK_FLAGS.NORMAL, out stringsBank);
                StudioSystem.loadBankFile($"Content/audio/{bankName}.bank", LOAD_BANK_FLAGS.NORMAL, out mainBank);
                mainBank.loadSampleData();

                if (!masterBank.isValid())
                    Debug.Log("Master bank cannot be found", LogType.Warning);
                if (!mainBank.isValid())
                    Debug.Log(bankName + " bank cannot be found", LogType.Warning);
            }

            oldBankName = bankName;
        }

        public static void Update(Vector2 listenerPos)
        {
            foreach(KeyValuePair<int, EventInstance> instk in Instances)
            {
                var info = EventInfo[instk.Key];
                var instance = instk.Value;
                if (info.Reverb && SpacialReverbs != null)
                {
                    foreach (ReverbZone reverbZone in SpacialReverbs)
                    {
                        float dist = Vector2.Distance(info.Position, reverbZone.Position);
                        if (dist < reverbZone.MaxRadius)
                        {
                            dist = Vector2.Distance(listenerPos, info.Position);
                            float wet = dist / reverbZone.MaxRadius * reverbZone.Wetness;
                            wet = dist < reverbZone.MinRadius ? 0f : wet;
                            wet = Math.Clamp(wet, 0f, 1f);
                            Instances[instk.Key].setReverbLevel(reverbZone.Type, wet);
                        }
                    }
                }
                instance.getPlaybackState(out var st);
                if (info.Release && st == PLAYBACK_STATE.STOPPED)
                {
                    Instances[instk.Key].release();
                    Instances.Remove(instk.Key);
                    EventInfo.Remove(instk.Key);
                }
            }
            StudioSystem.setListenerAttributes(0, GetPositioned3DA(listenerPos));
            StudioSystem.update();
            FMODSystem.update();

            mainBank.getSampleLoadingState(out var state);
            canPlaySound = state == LOADING_STATE.LOADED;
        }

        private static int counter = -1;

        public static int PlayEvent(string path, bool release, Vector2 pos, bool reverb, float vol = 1f, bool force3D = true, float pitch = 1f)
        {
            if (canPlaySound)
            {
                StudioSystem.getEvent("event:/" + path, out var sEvent);
                sEvent.createInstance(out var inst);

                inst.getPlaybackState(out PLAYBACK_STATE state);
                if (state != PLAYBACK_STATE.PLAYING)
                {
                    inst.setVolume(vol);
                    inst.setPitch(pitch);
                    if (force3D)
                        inst.set3DAttributes(GetPositioned3DA(pos));
                    inst.start();
                    if (release)
                        inst.release();
                    counter++;
                    Instances.Add(counter, inst);
                    EventInfo.Add(counter, new EventSoundInfo(pos, reverb, force3D, release));
                    return counter;
                }
            }
            else
                Debug.Log("Cannot play sound of: " + path + " due to unloaded bank samples!", LogType.Warning);
            return -1;
        }

        public static void StopEvent(int eventID, bool fade)
        {
            Instances[eventID].stop(fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            Instances[eventID].release();
        }

        public static void StopAllEvents()
        {
            foreach (KeyValuePair<int, EventInstance> instk in Instances)
            {
                Instances[instk.Key].stop(STOP_MODE.ALLOWFADEOUT);
                Instances[instk.Key].release();
                Instances.Remove(instk.Key);
                EventInfo.Remove(instk.Key);
            }
        }

        public static void PauseEvent(int eventID, bool state)
        {
            Instances[eventID].setPaused(state);
        }

        public static void SetEventPosition(int eventID, Vector2 pos)
        {
            if (EventInfo[eventID].Force3D)
                Instances[eventID].set3DAttributes(GetPositioned3DA(pos));
        }

        public static void SetParameter(int eventID, string name, float value)
        {
            Instances[eventID].setParameterByName(name, value);
        }
        public static float GetParameter(int eventID, string name)
        {
            Instances[eventID].getParameterByName(name, out float ret);
            return ret;
        }

        private static ATTRIBUTES_3D GetPositioned3DA(Vector2 pos)
        {
            return new ATTRIBUTES_3D
            {
                position = new VECTOR { x = pos.X, y = pos.Y, z = 0f },
                velocity = new VECTOR { x = 0f, y = 0f, z = 0f },
                forward = new VECTOR { x = 0f, y = 0f, z = 1f },
                up = new VECTOR { x = 0f, y = 1f, z = 0f }
            };
        }
    }

    public struct ReverbZone {
        public Vector2 Position { get; }
        public float MinRadius { get; }
        public float MaxRadius { get; }
        public int Type { get; }
        public float Wetness { get; set; }

        public ReverbZone(Vector2 pos, float minRad, float maxRad, int t, float wet)
        {
            Position = pos;
            MinRadius = minRad;
            MaxRadius = maxRad;
            Type = t;
            Wetness = wet;
        }
    }

    public struct EventSoundInfo {
        public Vector2 Position { get; set; }
        public bool Reverb { get; }
        public bool Force3D { get; }
        public bool Release { get; }

        public EventSoundInfo(Vector2 pos, bool rev, bool threeD, bool rel)
        {
            Position = pos;
            Reverb = rev;
            Force3D = threeD;
            Release = rel;
        }
    }
}