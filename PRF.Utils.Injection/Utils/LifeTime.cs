namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// Les différents types de durée de vie possibles pour les enregistrements d'une classe dans le conteneur d'injection
    /// </summary>
    public enum LifeTime
    {
        /// <summary>
        /// La classe sera crée sous forme de singleton et la même instance sera renvoyé à chaque demande par le conteneur (comportement le plus courant)
        /// </summary>
        Singleton,

        /// <summary>
        /// le conteneur renverra une nouvelle instance de la classe à chaque demande faite par le consommateur
        /// </summary>
        Transient
    }
}
