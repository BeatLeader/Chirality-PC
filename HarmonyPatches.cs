﻿using Chirality.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace Chirality
{
    [HarmonyPatch (typeof(StandardLevelDetailView), "SetContent")]
    internal class StandardLevelDetailViewPatch
    {
        static void Prefix(IBeatmapLevel level)
        {
            Plugin.Log.Debug("SetContent");

            List<IDifficultyBeatmapSet> custom_difficultyBeatmapSets = new List<IDifficultyBeatmapSet>(level.beatmapLevelData.difficultyBeatmapSets);

            if (level.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault() == null)
            {
                return;
            }


            if (level.beatmapLevelData.difficultyBeatmapSets.Any((i) => i.beatmapCharacteristic.serializedName == "Horizontal"))
            {
                return;
            }


            int index = -1;

            for (int i = 0; i < level.beatmapLevelData.difficultyBeatmapSets.Length; i++)
            {
                if (level.beatmapLevelData.difficultyBeatmapSets[i].beatmapCharacteristic.serializedName == "Lawless")
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return;
            }    


            // Later add other modes
            /*if (level.beatmapLevelData.difficultyBeatmapSets.Any((i) => i.beatmapCharacteristic.serializedName == "Standard" && PluginConfig.Instance.mode == "Standard"))
            {
                Plugin.Log.Debug("Any difficultyBeatmapSets: " + level.beatmapLevelData.difficultyBeatmapSets[0].beatmapCharacteristic.serializedName);
            }*/


            CustomDifficultyBeatmapSet h_beatmapset = new CustomDifficultyBeatmapSet(Create_BMCSO("Horizontal", "Mirror Left-Right"));
            CustomDifficultyBeatmapSet v_beatmapset = new CustomDifficultyBeatmapSet(Create_BMCSO("Vertical", "Mirror Up-Down"));
            CustomDifficultyBeatmapSet i_beatmapset = new CustomDifficultyBeatmapSet(Create_BMCSO("Invert", "Invert"));


            CustomDifficultyBeatmap[] h_customDifficultyBeatmaps = level.beatmapLevelData.difficultyBeatmapSets[index].difficultyBeatmaps.Select((i) => new CustomDifficultyBeatmap(i.level, h_beatmapset, i.difficulty, i.difficultyRank, i.noteJumpMovementSpeed, i.noteJumpStartBeatOffset, MirrorTransforms.Mirror_Horizontal(i.beatmapData.GetCopy()))).ToArray();
            CustomDifficultyBeatmap[] v_customDifficultyBeatmaps = level.beatmapLevelData.difficultyBeatmapSets[index].difficultyBeatmaps.Select((i) => new CustomDifficultyBeatmap(i.level, h_beatmapset, i.difficulty, i.difficultyRank, i.noteJumpMovementSpeed, i.noteJumpStartBeatOffset, MirrorTransforms.Mirror_Vertical(i.beatmapData.GetCopy()))).ToArray();
            CustomDifficultyBeatmap[] i_customDifficultyBeatmaps = level.beatmapLevelData.difficultyBeatmapSets[index].difficultyBeatmaps.Select((i) => new CustomDifficultyBeatmap(i.level, h_beatmapset, i.difficulty, i.difficultyRank, i.noteJumpMovementSpeed, i.noteJumpStartBeatOffset, MirrorTransforms.Mirror_Inverse(i.beatmapData.GetCopy()))).ToArray();


            h_beatmapset.SetCustomDifficultyBeatmaps(h_customDifficultyBeatmaps);
            v_beatmapset.SetCustomDifficultyBeatmaps(v_customDifficultyBeatmaps);
            i_beatmapset.SetCustomDifficultyBeatmaps(i_customDifficultyBeatmaps);


            custom_difficultyBeatmapSets.Add(h_beatmapset);
            custom_difficultyBeatmapSets.Add(v_beatmapset);
            custom_difficultyBeatmapSets.Add(i_beatmapset);


            if (level.beatmapLevelData is BeatmapLevelData beatmapLevelData)
            {
                Set_Field(beatmapLevelData, "_difficultyBeatmapSets", custom_difficultyBeatmapSets.ToArray());
            }
        }


        internal static BeatmapCharacteristicSO Create_BMCSO(string name, string hint)
        {
            SpriteRenderer[] sprites = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            
            SpriteRenderer a = null;
            foreach (var s in sprites)
            {
                Plugin.Log.Debug(s.name);
                if (s.name == "NoteArrowGlow")
                    a = s;
            }

            

            //Texture2D texture = new Texture2D(50, 50);
            //Sprite icon = Sprite.Create(texture, new Rect(0, 0, 50, 50), new Vector2(0.5f, 0.5f));

            Sprite icon = SongCore.Utilities.Utils.LoadSpriteFromFile("RedX.png");

            BeatmapCharacteristicSO beatmapCharacteristicSO = BeatmapCharacteristicSO.CreateInstance<BeatmapCharacteristicSO>();

            Set_Field(beatmapCharacteristicSO, "_icon", icon);
            Set_Field(beatmapCharacteristicSO, "_characteristicNameLocalizationKey", name);
            Set_Field(beatmapCharacteristicSO, "_descriptionLocalizationKey", hint);
            Set_Field(beatmapCharacteristicSO, "_serializedName", name);
            Set_Field(beatmapCharacteristicSO, "_compoundIdPartName", name);
            Set_Field(beatmapCharacteristicSO, "_sortingOrder", 100);
            Set_Field(beatmapCharacteristicSO, "_containsRotationEvents", false);
            Set_Field(beatmapCharacteristicSO, "_requires360Movement", false);
            Set_Field(beatmapCharacteristicSO, "_numberOfColors", 2);

            return beatmapCharacteristicSO;
        }


        internal static void Set_Field(object obj, string fieldName, object value)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }
    }
}