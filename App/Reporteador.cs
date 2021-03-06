using CoreEscuela.Entidades;
using System.Linq;
namespace CoreEscuela.App
{
    public class Reporteador
    {
        Dictionary<LlaveDiccionario, IEnumerable<ObjetoEscuelaBase>> _diccionario;
        public Reporteador(Dictionary<LlaveDiccionario, IEnumerable<ObjetoEscuelaBase>> dicObjsEscuela)
        {
            if (dicObjsEscuela == null)
            {
                throw new ArgumentException(nameof(dicObjsEscuela));
            }
            _diccionario = dicObjsEscuela;
        }

        public IEnumerable<Evaluacion> GetListaEvaluacions()
        {
            if (_diccionario.TryGetValue(LlaveDiccionario.Evaluacion, out IEnumerable<ObjetoEscuelaBase> lista))
            {
                return lista.Cast<Evaluacion>();
            }
            {
                return new List<Evaluacion>();
            }
        }
         public IEnumerable<string> GetListaAsignaturas()
        {
            return GetListaAsignaturas(out var dummy);   
        }
        public IEnumerable<string> GetListaAsignaturas(out IEnumerable<Evaluacion>  listaEv)
        {
            listaEv = GetListaEvaluacions();

            return (from Evaluacion ev in listaEv select ev.Asignatura.Nombre).Distinct();   
        }
        public Dictionary<string,IEnumerable<Evaluacion>> getDicEvaluacionXAsign(){
             
             Dictionary<string,IEnumerable<Evaluacion>> resDic = new  Dictionary<string,IEnumerable<Evaluacion>> ();
             
             var listaAsign = GetListaAsignaturas(out var listaEv);
             foreach (var asign in listaAsign)
             {
                 var evalsAsign = from eval in listaEv where eval.Asignatura.Nombre == asign select eval;
                 resDic.Add(asign,evalsAsign);
             }

             return resDic;
        }
        public Dictionary<string,IEnumerable<object>> getPromedioAlumnXAsignatura(){
            var res = new Dictionary<string,IEnumerable<object>>();
            var dicEvalXAsign = getDicEvaluacionXAsign();

            foreach (var asignConEval in dicEvalXAsign)
            {
                var promediosAlumn = from eval in asignConEval.Value 
                    group eval by new{
                        eval.Alumno.UniqueId,
                        eval.Alumno.Nombre
                        }
                    into grupoEvalsAlumno
                    select new AlumnoPromedio{
                        alumnoId = grupoEvalsAlumno.Key.UniqueId,
                        alumnoNombre = grupoEvalsAlumno.Key.Nombre,
                        promedio = grupoEvalsAlumno.Average(evaluacion => evaluacion.Nota)
                    };
                res.Add(asignConEval.Key,promediosAlumn);
            }
            return res;
        }
         public Dictionary<string, IEnumerable<AlumnoPromedio>> GetListaTopPromedio(int x)
        {
            var resp = new Dictionary<string, IEnumerable<AlumnoPromedio>>();
            var dicPromAlumPorAsignatura = getPromedioAlumnXAsignatura();

            foreach (var item in dicPromAlumPorAsignatura)
            {
                var dummy = (from AlumnoPromedio ap in item.Value
                             orderby ap.promedio descending
                             select ap).Take(x);

                resp.Add(item.Key, dummy);
            }
            return resp;
        }
    }
}