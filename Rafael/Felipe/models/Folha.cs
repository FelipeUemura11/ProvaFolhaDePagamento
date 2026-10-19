public class Folha
{
    public int Id {get;set;}
    public int Quantidade {get;set;}
    public double Valor {get;set;}
    public int Mes {get;set;}
    public int Ano {get;set;}

    public int FuncionarioId {get;set;}
    public Funcionario? Funcionario {get;set;}
}