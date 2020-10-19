using System;
using System.Collections.Generic;
using Hai.ComboGesture.Scripts.Editor.EditorUI.Effectors;
using UnityEngine;

namespace Hai.ComboGesture.Scripts.Editor.EditorUI
{
    public class CgeActivityEditorDriver
    {
        private static readonly Dictionary<string, string> Translations = new Dictionary<string, string>
        {
            {"anim00", "No gesture"},
            {"anim01", "FIST"},
            {"anim02", "OPEN"},
            {"anim03", "POINT"},
            {"anim04", "PEACE"},
            {"anim05", "ROCKNROLL"},
            {"anim06", "GUN"},
            {"anim07", "THUMBSUP"},
            {"anim11", "FIST x2, L+R trigger"},
            {"anim11_L", "FIST x2, LEFT trigger"},
            {"anim11_R", "FIST x2, RIGHT trigger"},
            {"anim12", "OPEN + FIST"},
            {"anim13", "POINT + FIST"},
            {"anim14", "PEACE + FIST"},
            {"anim15", "ROCKNROLL + FIST"},
            {"anim16", "GUN + FIST"},
            {"anim17", "THUMBSUP + FIST"},
            {"anim22", "OPEN x2"},
            {"anim23", "OPEN + POINT"},
            {"anim24", "OPEN + PEACE"},
            {"anim25", "OPEN + ROCKNROLL"},
            {"anim26", "OPEN + GUN"},
            {"anim27", "OPEN + THUMBSUP"},
            {"anim33", "POINT x2"},
            {"anim34", "POINT + PEACE"},
            {"anim35", "POINT + ROCKNROLL"},
            {"anim36", "POINT + GUN"},
            {"anim37", "POINT + THUMBSUP"},
            {"anim44", "PEACE x2"},
            {"anim45", "PEACE + ROCKNROLL"},
            {"anim46", "PEACE + GUN"},
            {"anim47", "PEACE + THUMBSUP"},
            {"anim55", "ROCKNROLL x2"},
            {"anim56", "ROCKNROLL + GUN"},
            {"anim57", "ROCKNROLL + THUMBSUP"},
            {"anim66", "GUN x2"},
            {"anim67", "GUN + THUMBSUP"},
            {"anim77", "THUMBSUP x2"},
            {"viseme0", "sil (0)"},
            {"viseme1", "PP (1)"},
            {"viseme2", "FF (2)"},
            {"viseme3", "TH (3)"},
            {"viseme4", "DD (4)"},
            {"viseme5", "kk (5)"},
            {"viseme6", "CH (6)"},
            {"viseme7", "SS (7)"},
            {"viseme8", "nn (8)"},
            {"viseme9", "RR (9)"},
            {"viseme10", "aa (10)"},
            {"viseme11", "E (11)"},
            {"viseme12", "ih (12)"},
            {"viseme13", "oh (13)"},
            {"viseme14", "ou (14)"},

            {"p_anim00", "None x2"},
            {"p_anim01", "None > FIST"},
            {"p_anim02", "None > OPEN"},
            {"p_anim03", "None > POINT"},
            {"p_anim04", "None > PEACE"},
            {"p_anim05", "None > ROCKNROLL"},
            {"p_anim06", "None > GUN"},
            {"p_anim07", "None > THUMBSUP"},

            {"p_anim10", "FIST > None"},
            {"p_anim11", "FIST x2"},
            {"p_anim11_L", "FIST x2, LEFT trigger"},
            {"p_anim11_R", "FIST x2, RIGHT trigger"},
            {"p_anim12", "FIST > OPEN"},
            {"p_anim13", "FIST > POINT"},
            {"p_anim14", "FIST > PEACE"},
            {"p_anim15", "FIST > ROCKNROLL"},
            {"p_anim16", "FIST > GUN"},
            {"p_anim17", "FIST > THUMBSUP"},

            {"p_anim20", "OPEN > None"},
            {"p_anim21", "OPEN > FIST"},
            {"p_anim22", "OPEN x2"},
            {"p_anim23", "OPEN > POINT"},
            {"p_anim24", "OPEN > PEACE"},
            {"p_anim25", "OPEN > ROCKNROLL"},
            {"p_anim26", "OPEN > GUN"},
            {"p_anim27", "OPEN > THUMBSUP"},

            {"p_anim30", "POINT > None"},
            {"p_anim31", "POINT > FIST"},
            {"p_anim32", "POINT > OPEN"},
            {"p_anim33", "POINT x2"},
            {"p_anim34", "POINT > PEACE"},
            {"p_anim35", "POINT > ROCKNROLL"},
            {"p_anim36", "POINT > GUN"},
            {"p_anim37", "POINT > THUMBSUP"},

            {"p_anim40", "PEACE > None"},
            {"p_anim41", "PEACE > FIST"},
            {"p_anim42", "PEACE > OPEN"},
            {"p_anim43", "PEACE > POINT"},
            {"p_anim44", "PEACE x2"},
            {"p_anim45", "PEACE > ROCKNROLL"},
            {"p_anim46", "PEACE > GUN"},
            {"p_anim47", "PEACE > THUMBSUP"},

            {"p_anim50", "ROCKNROLL > None"},
            {"p_anim51", "ROCKNROLL > FIST"},
            {"p_anim52", "ROCKNROLL > OPEN"},
            {"p_anim53", "ROCKNROLL > POINT"},
            {"p_anim54", "ROCKNROLL > PEACE"},
            {"p_anim55", "ROCKNROLL x2"},
            {"p_anim56", "ROCKNROLL > GUN"},
            {"p_anim57", "ROCKNROLL > THUMBSUP"},


            {"p_anim60", "GUN > None"},
            {"p_anim61", "GUN > FIST"},
            {"p_anim62", "GUN > OPEN"},
            {"p_anim63", "GUN > POINT"},
            {"p_anim64", "GUN > PEACE"},
            {"p_anim65", "GUN > ROCKNROLL"},
            {"p_anim66", "GUN x2"},
            {"p_anim67", "GUN > THUMBSUP"},

            {"p_anim70", "THUMBSUP > None"},
            {"p_anim71", "THUMBSUP > FIST"},
            {"p_anim72", "THUMBSUP > OPEN"},
            {"p_anim73", "THUMBSUP > POINT"},
            {"p_anim74", "THUMBSUP > PEACE"},
            {"p_anim75", "THUMBSUP > ROCKNROLL"},
            {"p_anim76", "THUMBSUP > GUN"},
            {"p_anim77", "THUMBSUP x2"},
        };

        private static readonly Dictionary<string, MergePair> ParameterToMerge = new Dictionary<string, MergePair>
        {
            {"anim12", new MergePair("anim01", "anim02")},
            {"anim13", new MergePair("anim01", "anim03")},
            {"anim14", new MergePair("anim01", "anim04")},
            {"anim15", new MergePair("anim01", "anim05")},
            {"anim16", new MergePair("anim01", "anim06")},
            {"anim17", new MergePair("anim01", "anim07")},
            {"anim23", new MergePair("anim02", "anim03")},
            {"anim24", new MergePair("anim02", "anim04")},
            {"anim25", new MergePair("anim02", "anim05")},
            {"anim26", new MergePair("anim02", "anim06")},
            {"anim27", new MergePair("anim02", "anim07")},
            {"anim34", new MergePair("anim03", "anim04")},
            {"anim35", new MergePair("anim03", "anim05")},
            {"anim36", new MergePair("anim03", "anim06")},
            {"anim37", new MergePair("anim03", "anim07")},
            {"anim45", new MergePair("anim04", "anim05")},
            {"anim46", new MergePair("anim04", "anim06")},
            {"anim47", new MergePair("anim04", "anim07")},
            {"anim56", new MergePair("anim05", "anim06")},
            {"anim57", new MergePair("anim05", "anim07")},
            {"anim67", new MergePair("anim06", "anim07")},


            {"anim21", new MergePair("anim10", "anim20")},
            {"anim31", new MergePair("anim10", "anim30")},
            {"anim41", new MergePair("anim10", "anim40")},
            {"anim51", new MergePair("anim10", "anim50")},
            {"anim61", new MergePair("anim10", "anim60")},
            {"anim71", new MergePair("anim10", "anim70")},
            {"anim32", new MergePair("anim20", "anim30")},
            {"anim42", new MergePair("anim20", "anim40")},
            {"anim52", new MergePair("anim20", "anim50")},
            {"anim62", new MergePair("anim20", "anim60")},
            {"anim72", new MergePair("anim20", "anim70")},
            {"anim43", new MergePair("anim30", "anim40")},
            {"anim53", new MergePair("anim30", "anim50")},
            {"anim63", new MergePair("anim30", "anim60")},
            {"anim73", new MergePair("anim30", "anim70")},
            {"anim54", new MergePair("anim40", "anim50")},
            {"anim64", new MergePair("anim40", "anim60")},
            {"anim74", new MergePair("anim40", "anim70")},
            {"anim65", new MergePair("anim50", "anim60")},
            {"anim75", new MergePair("anim50", "anim70")},
            {"anim76", new MergePair("anim60", "anim70")}
        };

        private static readonly Dictionary<string, MergePair> ParameterToMergePermutations = new Dictionary<string, MergePair>
        {
            {"anim11", new MergePair("anim01", "anim10")},
            {"anim22", new MergePair("anim02", "anim20")},
            {"anim33", new MergePair("anim03", "anim30")},
            {"anim44", new MergePair("anim04", "anim40")},
            {"anim55", new MergePair("anim05", "anim50")},
            {"anim66", new MergePair("anim06", "anim60")},
            {"anim77", new MergePair("anim07", "anim70")}
        };

        private static readonly Dictionary<string, string> AnimationToCopy = new Dictionary<string, string>
        {
            {"anim11", "anim01"},
            {"anim22", "anim02"},
            {"anim33", "anim03"},
            {"anim44", "anim04"},
            {"anim55", "anim05"},
            {"anim66", "anim06"},
            {"anim77", "anim07"},

            {"anim12", "anim02"},
            {"anim13", "anim03"},
            {"anim14", "anim04"},
            {"anim15", "anim05"},
            {"anim16", "anim06"},
            {"anim17", "anim07"},

            {"anim21", "anim20"},
            {"anim31", "anim30"},
            {"anim41", "anim40"},
            {"anim51", "anim50"},
            {"anim61", "anim60"},
            {"anim71", "anim70"},
        };

        private readonly CgeEditorEffector _editorEffector;

        public CgeActivityEditorDriver(CgeEditorEffector editorEffector)
        {
            _editorEffector = editorEffector;
        }

        public bool IsSymmetrical(string propertyPath)
        {
            return Translations[propertyPath].Contains("x2");
        }

        public string ShortTranslation(string propertyPath)
        {
            return Translations[propertyPath];
        }

        public bool IsAPropertyThatCanBeCombined(string propertyPath, bool usePermutations = false)
        {
            return ParameterToMerge.ContainsKey(propertyPath) || usePermutations && ParameterToMergePermutations.ContainsKey(propertyPath);
        }

        public bool AreCombinationSourcesDefinedAndCompatible(string propertyPath, bool usePermutations = false)
        {
            if (!IsAPropertyThatCanBeCombined(propertyPath, usePermutations))
            {
                return false;
            }

            var mergePair = !usePermutations
                ? ParameterToMerge[propertyPath]
                : (ParameterToMerge.ContainsKey(propertyPath) ? ParameterToMerge[propertyPath] : ParameterToMergePermutations[propertyPath]);
            var left = _editorEffector.SpProperty(mergePair.Left).objectReferenceValue;
            var right = _editorEffector.SpProperty(mergePair.Right).objectReferenceValue;

            return left is AnimationClip && right is AnimationClip && left != right;
        }

        public bool AreCombinationSourcesIdentical(string propertyPath)
        {
            if (!IsAPropertyThatCanBeCombined(propertyPath))
            {
                return false;
            }

            var mergePair = ParameterToMerge[propertyPath];
            var left = _editorEffector.SpProperty(mergePair.Left).objectReferenceValue;
            var right = _editorEffector.SpProperty(mergePair.Right).objectReferenceValue;

            return left != null && right != null && left == right;
        }

        public MergePair ProvideCombinationPropertySources(string propertyPath)
        {
            if (!IsAPropertyThatCanBeCombined(propertyPath))
            {
                throw new ArgumentException();
            }

            return ParameterToMerge[propertyPath];
        }

        public bool IsAutoSettable(string propertyPath)
        {
            return AnimationToCopy.ContainsKey(propertyPath);
        }

        public string GetAutoSettableSource(string propertyPath)
        {
            return AnimationToCopy[propertyPath];
        }
    }

    public readonly struct MergePair
    {
        public MergePair(string left, string right)
        {
            Left = left;
            Right = right;
        }

        internal string Left { get; }
        internal string Right { get; }
    }
}
