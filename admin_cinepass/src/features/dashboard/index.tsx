
import { 
  Bar, 
  BarChart, 
  ResponsiveContainer, 
  XAxis, 
  YAxis, 
  Tooltip,
  CartesianGrid 
} from 'recharts';
import { 
  CreditCard, 
  DollarSign, 
  Film, 
  Ticket, 
  Users, 
  Activity 
} from 'lucide-react';

// Shadcn UI Components
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

// --- MOCK DATA ---
const REVENUE_DATA = [
  { name: 'T1', total: 4500 },
  { name: 'T2', total: 3200 },
  { name: 'T3', total: 7800 }, // Tháng cao điểm
  { name: 'T4', total: 5600 },
  { name: 'T5', total: 4800 },
  { name: 'T6', total: 9200 }, // Bom tấn mùa hè
  { name: 'T7', total: 8500 },
  { name: 'T8', total: 6700 },
  { name: 'T9', total: 5400 },
  { name: 'T10', total: 7200 },
  { name: 'T11', total: 6100 },
  { name: 'T12', total: 10500 }, // Mùa lễ hội
];

const RECENT_SALES = [
  {
    name: "Nguyễn Văn A",
    email: "nguyenvana@example.com",
    amount: "180.000đ",
    movie: "Dune: Part Two",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Felix"
  },
  {
    name: "Trần Thị B",
    email: "tranthib@example.com",
    amount: "240.000đ",
    movie: "Kung Fu Panda 4",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Aneka"
  },
  {
    name: "Lê Văn C",
    email: "levanc@example.com",
    amount: "90.000đ",
    movie: "Mai",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Milo"
  },
  {
    name: "Phạm Thị D",
    email: "phamthid@example.com",
    amount: "320.000đ",
    movie: "Godzilla x Kong",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=Bella"
  },
  {
    name: "Hoàng Văn E",
    email: "hoangvane@example.com",
    amount: "150.000đ",
    movie: "Exhuma",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=George"
  },
];

// --- COMPONENTS ---

// 1. Component hiển thị thẻ chỉ số nhỏ (Stats Card)
const StatsCard = ({ title, value, subText, icon: Icon }: any) => (
  <Card>
    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
      <CardTitle className="text-sm font-medium">{title}</CardTitle>
      <Icon className="h-4 w-4 text-muted-foreground" />
    </CardHeader>
    <CardContent>
      <div className="text-2xl font-bold">{value}</div>
      <p className="text-xs text-muted-foreground">{subText}</p>
    </CardContent>
  </Card>
);

// 2. Trang Dashboard Chính
const DashboardPage = () => {
  return (
    <div className="flex-1 space-y-4 p-4 md:p-8 pt-6">
      {/* Header */}
      <div className="flex items-center justify-between space-y-2">
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
        <div className="flex items-center space-x-2">
          <Button>Tải báo cáo</Button>
        </div>
      </div>

      {/* Tabs chuyển đổi view (Tổng quan / Analytics / Reports) */}
      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Tổng quan</TabsTrigger>
          <TabsTrigger value="analytics" disabled>Phân tích</TabsTrigger>
          <TabsTrigger value="reports" disabled>Báo cáo</TabsTrigger>
        </TabsList>
        
        <TabsContent value="overview" className="space-y-4">
          
          {/* Hàng 1: 4 Thẻ Stats quan trọng */}
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <StatsCard 
              title="Tổng doanh thu" 
              value="452.310.000đ" 
              subText="+20.1% so với tháng trước" 
              icon={DollarSign} 
            />
            <StatsCard 
              title="Vé đã bán" 
              value="+2350" 
              subText="+180% so với tháng trước" 
              icon={Ticket} 
            />
            <StatsCard 
              title="Phim đang chiếu" 
              value="12" 
              subText="2 phim sắp ra mắt tuần này" 
              icon={Film} 
            />
            <StatsCard 
              title="Khách hàng mới" 
              value="+573" 
              subText="+201 kể từ giờ trước" 
              icon={Activity} 
            />
          </div>

          {/* Hàng 2: Biểu đồ & Danh sách giao dịch */}
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
            
            {/* Cột trái (chiếm 4/7): Biểu đồ doanh thu */}
            <Card className="col-span-4">
              <CardHeader>
                <CardTitle>Biểu đồ doanh thu</CardTitle>
                <CardDescription>
                    Tổng doanh thu năm 2025 (Đơn vị: Triệu VNĐ)
                </CardDescription>
              </CardHeader>
              <CardContent className="pl-2">
                <ResponsiveContainer width="100%" height={350}>
                  <BarChart data={REVENUE_DATA}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} className="stroke-muted" />
                    <XAxis 
                      dataKey="name" 
                      stroke="#888888" 
                      fontSize={12} 
                      tickLine={false} 
                      axisLine={false} 
                    />
                    <YAxis 
                      stroke="#888888" 
                      fontSize={12} 
                      tickLine={false} 
                      axisLine={false} 
                      tickFormatter={(value) => `${value}Tr`} 
                    />
                    <Tooltip 
                        cursor={{fill: 'transparent'}}
                        contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}
                    />
                    <Bar 
                        dataKey="total" 
                        fill="currentColor" 
                        radius={[4, 4, 0, 0]} 
                        className="fill-primary" 
                    />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            {/* Cột phải (chiếm 3/7): Giao dịch gần đây */}
            <Card className="col-span-3">
              <CardHeader>
                <CardTitle>Giao dịch gần đây</CardTitle>
                <CardDescription>
                  Bạn có 265 vé được bán trong hôm nay.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-8">
                  {RECENT_SALES.map((sale, index) => (
                    <div key={index} className="flex items-center">
                      <Avatar className="h-9 w-9">
                        <AvatarImage src={sale.avatar} alt="Avatar" />
                        <AvatarFallback>KH</AvatarFallback>
                      </Avatar>
                      <div className="ml-4 space-y-1">
                        <p className="text-sm font-medium leading-none">{sale.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {sale.movie}
                        </p>
                      </div>
                      <div className="ml-auto font-medium">{sale.amount}</div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default DashboardPage;