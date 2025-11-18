using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filas
{
    internal class Program
    {
        static string conexaobd = "server=localhost;user=root;pwd=root;database=atendimento;port=3308";

        static void Main(string[] args)
        {
            int opcao = 0;

            do
            {
                Console.WriteLine("Selecione uma opção");
                Console.WriteLine("1. Cadastrar paciente");
                Console.WriteLine("2. Lista pacientes");
                Console.WriteLine("3. Atender próximo paciente");
                Console.WriteLine("4. Alterar dados do paciente");
                Console.WriteLine("0. Sair");
                Console.Write("Opção: ");

                opcao = int.Parse(Console.ReadLine());

                switch (opcao)
                {
                    case 1: CadastrarPaciente(); break;
                    case 2: ListaPacientes(); break;
                    case 3: ChamarProximo(); break;
                    case 4: AlteraPaciente(); break;
                    case 0: Console.WriteLine("Encerrando..."); break;
                    default: Console.WriteLine("Opção inválida!"); break;
                }
            } while (opcao != 0);
        }


        static void CadastrarPaciente()
        {
            Console.Write("Nome do paciente: ");
            string nome = Console.ReadLine();
            Console.Write("Idade do paciente: ");
            string idade = Console.ReadLine();
            Console.Write("Paciente com deficiência? (y/n): ");
            string resposta = Console.ReadLine().Trim().ToLower();

            int prioridade;

            if (resposta == "y")
            {
                prioridade = 2;
            }
            else
            {
                prioridade = 1;
            }

            MySqlConnection conexao = new MySqlConnection(conexaobd);

            try
            {
                conexao.Open();
                string sql = "INSERT INTO pacientes(nome,idade,prioridade,chamada) VALUES (@nome,@idade,@prioridade,@chamada)";

                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@idade", idade);
                cmd.Parameters.AddWithValue("@prioridade", prioridade);
                cmd.Parameters.AddWithValue("@chamada", "0");

                int linhas = cmd.ExecuteNonQuery();
                Console.WriteLine($"Paciente Cadastrado: {linhas}");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error" + ex.Message);
                return;
            }
            finally
            {
                conexao.Close();
            }
        }


        static void ListaPacientes()
        {
            MySqlConnection conexao = new MySqlConnection(conexaobd);

            try
            {
                conexao.Open();

                string sql = "SELECT * FROM pacientes";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader linha = cmd.ExecuteReader();

                Console.WriteLine("\nLista de Pacientes:");
                while (linha.Read())
                {
                    Console.WriteLine($"ID: {linha["id"]}, Nome: {linha["nome"]}, Idade: {linha["idade"]}, Prioridade: {linha["prioridade"]}, Chamada: {linha["chamada"]}");
                }

                linha.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error" + ex.Message);
                return;
            }
            finally
            {
                conexao.Close();
            }
        }


        static void ChamarProximo()
        {
            MySqlConnection conexao = new MySqlConnection(conexaobd);

            try
            {
                conexao.Open();

                string sqlup = "UPDATE pacientes set chamada = 2 where chamada = 1";


                MySqlCommand cmdantigo = new  MySqlCommand(sqlup, conexao);
                cmdantigo.ExecuteNonQuery();


            
                string sql = "SELECT * FROM pacientes WHERE chamada = 0 ORDER BY prioridade DESC, id ASC LIMIT 1";

                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    Console.WriteLine("Nenhum paciente aguardando...");
                    reader.Close();
                    return;
                }

                int idPaciente = Convert.ToInt32(reader["id"]);
                string nome = reader["nome"].ToString();
                int prioridade = Convert.ToInt32(reader["prioridade"]);

               
                reader.Close();

                string atualiza = "UPDATE pacientes SET chamada = 1 WHERE id = @id";
                MySqlCommand cmd2 = new MySqlCommand(atualiza, conexao);
                cmd2.Parameters.AddWithValue("@id", idPaciente);
                cmd2.ExecuteNonQuery();

                Console.WriteLine($"Próximo paciente chamado:");
                Console.WriteLine($"ID: {idPaciente}, Nome: {nome}, Prioridade: {prioridade}");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return;
            }
            finally
            {
                conexao.Close();
            }
        }

        static void AlteraPaciente()
        {
            MySqlConnection conexao = new MySqlConnection(conexaobd);

            try
            {
                conexao.Open();

                string sqlSel = "SELECT * FROM pacientes WHERE chamada = 1 LIMIT 1";
                MySqlCommand cmdSel = new MySqlCommand(sqlSel, conexao);
                MySqlDataReader reader = cmdSel.ExecuteReader();

                if (!reader.Read())
                {
                    Console.WriteLine("Nenhum paciente está sendo atendido no momento.");
                    reader.Close();
                    return;
                }

                int idPaciente = Convert.ToInt32(reader["id"]);
                string nomeAtual = reader["nome"].ToString();
                string idadeAtual = reader["idade"].ToString();
                int prioridadeAtual = Convert.ToInt32(reader["prioridade"]);

                reader.Close();

                Console.WriteLine("Alterando o paciente atual:");
                Console.WriteLine($"id: {idPaciente}");
                Console.WriteLine($"Nome: {nomeAtual}");
                Console.WriteLine($"Idade: {idadeAtual}");
                Console.WriteLine($"Prioridade: {prioridadeAtual}");

                Console.Write("Novo nome: ");
                string novoNome = Console.ReadLine();

                Console.Write("Nova idade: ");
                string novaIdade = Console.ReadLine();

                Console.Write("Possui deficiência? (y/n): ");
                string resp = Console.ReadLine().Trim().ToLower();

                int prioridade;

                if (resp == "y")
                {
                    prioridade = 2;
                }
                else
                {
                    prioridade = 1;
                }


                string sqlUp = "UPDATE pacientes SET nome = @nome, idade = @idade, prioridade = @prioridade WHERE id = @id";
                MySqlCommand cmdUp = new MySqlCommand(sqlUp, conexao);
                cmdUp.Parameters.AddWithValue("@nome", novoNome);
                cmdUp.Parameters.AddWithValue("@idade", novaIdade);
                cmdUp.Parameters.AddWithValue("@prioridade", prioridade);
                cmdUp.Parameters.AddWithValue("@id", idPaciente);

                cmdUp.ExecuteNonQuery();

                Console.WriteLine("\nPaciente atualizado com sucesso!");

            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return;
            }
            finally
            {
                conexao.Close();
            }
        }
    }
}
