using System;
using System.Collections.Generic;
using System.Threading;
using FinalWar;
using System.IO;

namespace FinalWar_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Init(Console.WriteLine);

            ConfigDictionary.Instance.LoadLocalConfig("local.xml");

            using (FileStream fs = new FileStream(Path.Combine(ConfigDictionary.Instance.random_path, "random.dat"), FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    BattleRandomPool.Load(br);
                }
            }

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

            StaticData.Load<SkillSDS>("skill");

            Dictionary<int, SkillSDS> skillDic = StaticData.GetDic<SkillSDS>();

            StaticData.Load<AuraSDS>("aura");

            Dictionary<int, AuraSDS> auraDic = StaticData.GetDic<AuraSDS>();

            Battle.Init(mapDic, heroDic, skillDic, auraDic, effectDic);

            MapSDS.Load(null);

            Server<PlayerUnit> server = new Server<PlayerUnit>();

            server.Start("0.0.0.0", ConfigDictionary.Instance.port, 100);

            while (true)
            {
                server.Update();

                Thread.Sleep(10);
            }
        }
    }
}
