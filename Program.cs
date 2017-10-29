using System;
using System.Collections.Generic;
using System.Threading;
using FinalWar;
using System.IO;
using Connection;

namespace FinalWar_server
{
    class Program
    {
        private const int tickTime = 50;

        static void Main(string[] args)
        {
            Connection.Log.Init(Console.WriteLine);

            FinalWar.Log.Init(Console.WriteLine);

            ResourceLoad();

            Server<PlayerUnit> server = new Server<PlayerUnit>();

            server.Start("0.0.0.0", ConfigDictionary.Instance.port, 100, 12000);

            while (true)
            {
                long tick = server.Update();

                BattleManager.Instance.Update(tick);

                Thread.Sleep(tickTime);
            }
        }

        private static void ResourceLoad()
        {
            ConfigDictionary.Instance.LoadLocalConfig("local.xml");

            StaticData.path = ConfigDictionary.Instance.table_path;

            StaticData.Load<MapSDS>("map");

            StaticData.Load<TestCardsSDS>("testCards");

            StaticData.Load<HeroTypeSDS>("heroType");

            StaticData.Load<MapSDS>("map");

            Dictionary<int, MapSDS> mapDic = StaticData.GetDic<MapSDS>();

            StaticData.Load<HeroSDS>("hero");

            Dictionary<int, HeroSDS> heroDic = StaticData.GetDic<HeroSDS>();

            StaticData.Load<EffectSDS>("effect");

            Dictionary<int, EffectSDS> effectDic = StaticData.GetDic<EffectSDS>();

            StaticData.Load<AuraSDS>("aura");

            Dictionary<int, AuraSDS> auraDic = StaticData.GetDic<AuraSDS>();

            Battle.Init(mapDic, heroDic, auraDic, effectDic);

            MapSDS.Load(null);

            string actionStr = File.ReadAllText(ConfigDictionary.Instance.ai_path + "ai_action.xml");
            string summonStr = File.ReadAllText(ConfigDictionary.Instance.ai_path + "ai_summon.xml");

            BattleAi.Init(actionStr, summonStr);
        }
    }
}
