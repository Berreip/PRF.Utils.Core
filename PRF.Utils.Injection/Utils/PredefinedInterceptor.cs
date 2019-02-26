namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// La liste de tous les intercepteurs pré-configurés car d'usage classique. Il est possible de définir un intercepteur custom en
    /// surchargeant l'interface IInterceptor
    /// </summary>
    public enum PredefinedInterceptors
    {
        /// <summary>
        /// Intercepteur qui trace le début et la fin de chaque méthode interceptée et fait un ToString sur les paramètres de cette méthode
        /// </summary>
        MethodTraceInterceptor,

        /// <summary>
        /// Intercepteur qui trace le temps passé dans chaque méthode interceptée
        /// </summary>
        TimeWatchInterceptor
    }
}
