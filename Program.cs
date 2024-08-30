using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class Restaurante
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Endereco { get; set; }
    public string Telefone { get; set; }
}

public class Prato
{
    public int Id { get; set; }
    public int RestauranteId { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Endereco { get; set; }
    public string Telefone { get; set; }
}

public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int PratoId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
}

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int RestauranteId { get; set; }
    public DateTime DataPedido { get; set; }
    public string StatusPedido { get; set; }
    public decimal Total { get; set; }
    public List<ItemPedido> Itens { get; set; }
    public string ClienteNome { get; set; }
    public string RestauranteNome { get; set; }
}

public class AppIfood
{
    private string connectionString = "server=localhost;database=db_aulas_2024;user=gaby;password=1234567;SslMode=none;";

    public void CadastrarRestaurante(Restaurante restaurante)
    {
        string query = "INSERT INTO restaurantes (Nome, Endereco, Telefone) VALUES (@Nome, @Endereco, @Telefone)";
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nome", restaurante.Nome);
            cmd.Parameters.AddWithValue("@Endereco", restaurante.Endereco);
            cmd.Parameters.AddWithValue("@Telefone", restaurante.Telefone);
            cmd.ExecuteNonQuery();
        }
    }

    public void CadastrarPrato(Prato prato)
    {
        string checkQuery = "SELECT COUNT(*) FROM pratos WHERE Nome = @Nome AND RestauranteId = @RestauranteId";
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
            checkCmd.Parameters.AddWithValue("@Nome", prato.Nome);
            checkCmd.Parameters.AddWithValue("@RestauranteId", prato.RestauranteId);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
            {
                string query = "INSERT INTO pratos (RestauranteId, Nome, Descricao, Preco) VALUES (@RestauranteId, @Nome, @Descricao, @Preco)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RestauranteId", prato.RestauranteId);
                cmd.Parameters.AddWithValue("@Nome", prato.Nome);
                cmd.Parameters.AddWithValue("@Descricao", prato.Descricao);
                cmd.Parameters.AddWithValue("@Preco", prato.Preco);
                cmd.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("Prato já existe neste restaurante.");
            }
        }
    }

    public void CadastrarCliente(Cliente cliente)
    {
        string query = "INSERT INTO clientes (Nome, Endereco, Telefone) VALUES (@Nome, @Endereco, @Telefone)";
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nome", cliente.Nome);
            cmd.Parameters.AddWithValue("@Endereco", cliente.Endereco);
            cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
            cmd.ExecuteNonQuery();
        }
    }

    public void RealizarPedido(Pedido pedido)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            decimal total = 0;
            foreach (var item in pedido.Itens)
            {
                total += item.Preco * item.Quantidade;
            }

            string query = "INSERT INTO pedidos (ClienteId, RestauranteId, DataPedido, StatusPedido, Total) VALUES (@ClienteId, @RestauranteId, @DataPedido, @StatusPedido, @Total)";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ClienteId", pedido.ClienteId);
            cmd.Parameters.AddWithValue("@RestauranteId", pedido.RestauranteId);
            cmd.Parameters.AddWithValue("@DataPedido", pedido.DataPedido);
            cmd.Parameters.AddWithValue("@StatusPedido", pedido.StatusPedido);
            cmd.Parameters.AddWithValue("@Total", total);
            cmd.ExecuteNonQuery();

            long pedidoId = cmd.LastInsertedId;

            foreach (var item in pedido.Itens)
            {
                string itemQuery = "INSERT INTO itens_pedido (PedidoId, PratoId, Quantidade, Preco) VALUES (@PedidoId, @PratoId, @Quantidade, @Preco)";
                MySqlCommand itemCmd = new MySqlCommand(itemQuery, connection);
                itemCmd.Parameters.AddWithValue("@PedidoId", pedidoId);
                itemCmd.Parameters.AddWithValue("@PratoId", item.PratoId);
                itemCmd.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                itemCmd.Parameters.AddWithValue("@Preco", item.Preco);
                itemCmd.ExecuteNonQuery();
            }
        }
    }

    public List<Pedido> ListarPedidos()
    {
        List<Pedido> pedidos = new List<Pedido>();

        string query = "SELECT p.Id, p.DataPedido, p.StatusPedido, p.Total, c.Nome AS Cliente, r.Nome AS Restaurante " +
                       "FROM pedidos p " +
                       "JOIN clientes c ON p.ClienteId = c.Id " +
                       "JOIN restaurantes r ON p.RestauranteId = r.Id";

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Pedido pedido = new Pedido
                    {
                        Id = reader.GetInt32("Id"),
                        DataPedido = reader.GetDateTime("DataPedido"),
                        StatusPedido = reader.GetString("StatusPedido"),
                        Total = reader.GetDecimal("Total"),
                        ClienteNome = reader.GetString("Cliente"),
                        RestauranteNome = reader.GetString("Restaurante")
                    };
                    pedidos.Add(pedido);
                }
            }
        }
        return pedidos;
    }

    public void MostrarMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Cadastrar Restaurante");
            Console.WriteLine("2. Cadastrar Prato");
            Console.WriteLine("3. Cadastrar Cliente");
            Console.WriteLine("4. Realizar Pedido");
            Console.WriteLine("5. Listar Pedidos");
            Console.WriteLine("6. Sair");
            Console.Write("Escolha uma opção: ");

            string escolha = Console.ReadLine();
            switch (escolha)
            {
                case "1":
                    CadastrarRestauranteMenu();
                    break;
                case "2":
                    CadastrarPratoMenu();
                    break;
                case "3":
                    CadastrarClienteMenu();
                    break;
                case "4":
                    RealizarPedidoMenu();
                    break;
                case "5":
                    ListarPedidosMenu();
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }

    private void CadastrarRestauranteMenu()
    {
        Restaurante restaurante = new Restaurante();
        Console.Write("Nome do restaurante: ");
        restaurante.Nome = Console.ReadLine();
        Console.Write("Endereço: ");
        restaurante.Endereco = Console.ReadLine();
        Console.Write("Telefone: ");
        restaurante.Telefone = Console.ReadLine();
        CadastrarRestaurante(restaurante);
        Console.WriteLine("Restaurante cadastrado com sucesso!");
        Console.ReadKey();
    }

    private void CadastrarPratoMenu()
    {
        Prato prato = new Prato();
        Console.Write("Nome do prato: ");
        prato.Nome = Console.ReadLine();
        Console.Write("Descrição: ");
        prato.Descricao = Console.ReadLine();
        Console.Write("Preço: ");
        prato.Preco = Convert.ToDecimal(Console.ReadLine());

        Console.Write("ID do restaurante: ");
        prato.RestauranteId = Convert.ToInt32(Console.ReadLine());
        CadastrarPrato(prato);
        Console.WriteLine("Prato cadastrado com sucesso!");
        Console.ReadKey();
    }

    private void CadastrarClienteMenu()
    {
        Cliente cliente = new Cliente();
        Console.Write("Nome do cliente: ");
        cliente.Nome = Console.ReadLine();
        Console.Write("Endereço: ");
        cliente.Endereco = Console.ReadLine();
        Console.Write("Telefone: ");
        cliente.Telefone = Console.ReadLine();
        CadastrarCliente(cliente);
        Console.WriteLine("Cliente cadastrado com sucesso!");
        Console.ReadKey();
    }

    private void RealizarPedidoMenu()
    {
        Pedido pedido = new Pedido();
        pedido.Itens = new List<ItemPedido>();

        Console.Write("ID do Cliente: ");
        pedido.ClienteId = Convert.ToInt32(Console.ReadLine());
        Console.Write("ID do Restaurante: ");
        pedido.RestauranteId = Convert.ToInt32(Console.ReadLine());
        pedido.DataPedido = DateTime.Now;
        Console.Write("Status do Pedido: ");
        pedido.StatusPedido = Console.ReadLine();

        while (true)
        {
            ItemPedido itemPedido = new ItemPedido();
            Console.Write("ID do Prato: ");
            itemPedido.PratoId = Convert.ToInt32(Console.ReadLine());
            Console.Write("Quantidade: ");
            itemPedido.Quantidade = Convert.ToInt32(Console.ReadLine());
            Console.Write("Preço do Prato: ");
            itemPedido.Preco = Convert.ToDecimal(Console.ReadLine());
            pedido.Itens.Add(itemPedido);

            Console.Write("Adicionar outro item? (s/n): ");
            string adicionarOutro = Console.ReadLine();
            if (adicionarOutro.ToLower() != "s")
                break;
        }

        RealizarPedido(pedido);
        Console.WriteLine("Pedido realizado com sucesso!");
        Console.ReadKey();
    }

    private void ListarPedidosMenu()
    {
        List<Pedido> pedidos = ListarPedidos();

        Console.WriteLine("Pedidos:");
        foreach (var pedido in pedidos)
        {
            Console.WriteLine($"ID: {pedido.Id}, Data: {pedido.DataPedido}, Status: {pedido.StatusPedido}, Total: {pedido.Total:C}, Cliente: {pedido.ClienteNome}, Restaurante: {pedido.RestauranteNome}");
        }

        Console.ReadKey();
    }
}

class Program
{
    static void Main(string[] args)
    {
        AppIfood app = new AppIfood();
        app.MostrarMenu();
    }
}