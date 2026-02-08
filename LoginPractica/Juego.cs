namespace LoginPractica
{
    // Clase molde para mapear la tabla Juegos
    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public string Categoria { get; set; }
    }
}