namespace Duckov_RecipeRecordedIndicator
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private void Awake()
        {
            ModLogger.Log($"{Constant.ModName} Loaded");
        }

        private void OnEnable()
        {
            ModLoader.Initialize();
            HarmonyLoader.Initialize();
        }

        private void OnDisable()
        {
            ModLoader.Uninitialize();
            HarmonyLoader.Uninitialize();
        }

        private void OnDestroy()
        {
            ModLoader.Uninitialize();
            HarmonyLoader.Uninitialize();
        }
    }
}