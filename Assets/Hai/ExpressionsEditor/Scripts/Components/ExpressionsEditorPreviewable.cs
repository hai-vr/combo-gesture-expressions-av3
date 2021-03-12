using UnityEngine;

namespace Hai.ExpressionsEditor.Scripts.Components
{
    public abstract class ExpressionEditorPreviewable : MonoBehaviour
    {
        public abstract bool IsValid();
        public abstract EePreviewAvatar AsEePreviewAvatar();
        public abstract GameObject AsGameObject();
    }
}
