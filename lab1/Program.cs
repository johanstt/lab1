using System;
using System.Collections.Generic;
using System.Linq;

namespace LighthouseExpedition
{
    // интерфейс участников
     public interface IActor
    {
        string Name { get; }
        void Act(EnvironmentState env);
    }

    // исключения для обработки 
    public class RecoverableDomainException : Exception
    {
        public RecoverableDomainException(string message) : base(message) { }
    }

    public class FatalDomainException : Exception
    {
        public FatalDomainException(string message) : base(message) { }
    }

    // класс артефакта
    public record Artifact(string Id, string Name, ArtifactRarity Rarity);

    public enum ArtifactRarity
    {
        Common,
        Rare,
        Epic
    }

    // состояние окружения
    public class EnvironmentState
    {
        private Artifact[] _artifacts;
        public List<IActor> Actors { get; } = new();
        public List<Artifact> FoundArtifacts { get; } = new();
        public Random Random { get; } = new();
        public LogHelper Log { get; }

        public EnvironmentState(Artifact[] artifacts)
        {
            _artifacts = artifacts ?? Array.Empty<Artifact>();
            Log = new LogHelper();
        }

        // рандом арт
        public Artifact? PopRandomArtifact()
        {
            if (_artifacts == null || _artifacts.Length == 0)
                return null;

            int idx = Random.Next(_artifacts.Length);
            var art = _artifacts[idx];
            var temp = _artifacts.ToList();
            temp.RemoveAt(idx);
            _artifacts = temp.ToArray();
            return art;
        }

        // логи
        public class LogHelper
        {
            public void Write(string msg)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
            }
        }
    }

    // пример участников 
    public class Explorer : IActor
    {
        public string Name { get; }

        public Explorer(string name)
        {
            Name = name;
        }

        public void Act(EnvironmentState env)
        {
            var artifact = env.PopRandomArtifact();

            if (artifact == null)
            {
                env.Log.Write($"{Name} ничего не нашёл и вернулся в лагерь.");
                throw new RecoverableDomainException($"{Name} не смог найти артефакт.");
            }

            env.FoundArtifacts.Add(artifact);
            env.Log.Write($"{Name} нашёл артефакт: {artifact.Name} ({artifact.Rarity})");

            if (artifact.Rarity == ArtifactRarity.Epic)
                throw new FatalDomainException($"{Name} столкнулся с древним проклятием!");
        }
    }

    // енд
    internal class Program
    {
        static void Main()
        {
            var artifacts = new Artifact[]
            {
                new("a1", "Статуэтка маяка", ArtifactRarity.Common),
                new("a2", "Руна шторма", ArtifactRarity.Rare),
                new("a3", "Око Кракена", ArtifactRarity.Epic),
                new("a4", "Осколок линзы", ArtifactRarity.Common)
            };

            var env = new EnvironmentState(artifacts);

            env.Actors.Add(new Explorer("Алиса"));
            env.Actors.Add(new Explorer("Борис"));
            env.Actors.Add(new Explorer("Катя"));

            env.Log.Write("Экспедиция началась!");

            foreach (var actor in env.Actors)
            {
                try
                {
                    actor.Act(env);
                }
                catch (RecoverableDomainException ex)
                {
                    env.Log.Write($"⚠ {ex.Message}");
                }
                catch (FatalDomainException ex)
                {
                    env.Log.Write($"💀 {ex.Message}");
                    env.Log.Write("Экспедиция завершена из-за фатальной ошибки!");
                    break;
                }
            }

            env.Log.Write("\nИтоги экспедиции:");
            foreach (var art in env.FoundArtifacts)
                env.Log.Write($"• {art.Name} ({art.Rarity})");

            env.Log.Write("\nРабота завершена. Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}
