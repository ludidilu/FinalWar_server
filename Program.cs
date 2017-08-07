using System;
using System.Collections.Generic;
using System.Threading;
using FinalWar;

namespace FinalWar_server
{
    class Program
    {
        private static void WriteLog(string _str)
        {
            Console.WriteLine(_str);
        }

        static void Main(string[] args)
        {
            Log.Init(WriteLog);

            ConfigDictionary.Instance.LoadLocalConfig("local.xml");

            StaticData.path = ConfigDictionary.Instance.table_path;

            StaticData.Load<MapSDS>("map");

            Map.Init();

            StaticData.Load<TestCardsSDS>("testCards");

            StaticData.Load<HeroTypeSDS>("heroType");

            StaticData.Load<HeroSDS>("hero");

            Dictionary<int, HeroSDS> heroDic = StaticData.GetDic<HeroSDS>();

            StaticData.Load<EffectSDS>("effect");

            Dictionary<int, EffectSDS> effectDic = StaticData.GetDic<EffectSDS>();

            StaticData.Load<SkillSDS>("skill");

            Dictionary<int, SkillSDS> skillDic = StaticData.GetDic<SkillSDS>();

            StaticData.Load<AuraSDS>("aura");

            Dictionary<int, AuraSDS> auraDic = StaticData.GetDic<AuraSDS>();

            Battle.Init(Map.mapDataDic, heroDic, skillDic, auraDic, effectDic);

            Server<PlayerUnit> server = new Server<PlayerUnit>();

            server.Start("0.0.0.0", 1983, 100);

            while (true)
            {
                server.Update();

                Thread.Sleep(10);
            }
        }
    }
}
