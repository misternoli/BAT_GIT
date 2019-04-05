clear all; 
clc; 

filename = 'Matlab_0_01_to_0_5.xlsx';
sheet = 1;
fileende=23140;
Zeitende=fileende/2;
xlRange = 'A:E';
[num,txt,raw] = xlsread(filename,sheet,xlRange);
t= num(1:fileende,1); u= num(1:fileende,4); T_Zelle=num(1:fileende,2); T_Mantel=num(1:fileende,3) ;T_Umgebung=num(1:fileende,5);
%%
%In einem Plot
close all
figure('Name','Zelle');
subplot(2,1,1);
plot(t,T_Zelle,'r',t,T_Mantel,'b'); ylabel('T_Zelle');
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
title('Temperaturverlauf - Sprung von 0.005 auf 0.01')
legend('T_{Mantel}','T_{Zelle}','Location','southeast') ;legend('boxoff')

subplot(2,1,2);
plot(t,T_Umgebung);
title('Ambient Temperature ')
legend('T_{Umgebung}','Location','southeast') ;legend('boxoff')

%%
%Zwei Plots 
%Temperaturverlauf Zelle
close all
figure('Name','Temperaturverlauf Zelle');
plot(t,T_Zelle,'r'); ylabel('T_Zelle');
x0=0;
ymax=T_Zelle(fileende:fileende);
line([x0 Zeitende], [ymax ymax])
ymin=T_Zelle(1:1);
line([x0 Zeitende], [ymin ymin])
%Temperaturverlauf Mantel
figure('Name','Temperaturverlauf Mantel');
plot(t,T_Mantel,'r');% ylabel('T_Mantel')
ymax=T_Mantel(fileende:fileende);
line([x0 Zeitende], [ymax ymax])
ymin=T_Mantel(1:1);
line([x0 Zeitende], [ymin ymin])

%%
%Paramentersierung via SystemIdentification
%{
T_Mantelmin=90;     %Startwert beliebig um T_min herauszufinden
T_Zellemin=90;      %Startwert beliebig um T_min herauszufinden
for x=1:fileende
    if T_Mantelmin<T_Mantel(x:x)   
    else
        T_Mantelmin=T_Mantel(x:x);
    end
end
T_Mantel0 = T_Mantel-T_Mantelmin;

for x=1:fileende
    if T_Zellemin<T_Zelle(x:x)   
    else
        T_Zellemin=T_Zelle(x:x);
    end
end
T_Zelle0 = T_Zelle-T_Zellemin;
%}
figure(5)
T_Mantel0 = T_Mantel-T_Mantel(1:1);
T_Zelle0 = T_Zelle-T_Zelle(1:1);
plot(t,T_Mantel0,t,T_Zelle0)
%%
%-------------------------------
systemIdentification('Cooling_iden')
%%
%Mantelparameter einzeln betrachtet
Kp_Mantel=P2.KP;
T1_Mantel=P2.Tp1;
T2_Mantel=P2.Tp2;
G_Mantel= tf([Kp_Mantel],[T2_Mantel T1_Mantel 1])
%Bioreaktorparameter einzeln betrachtet
Kp_Reaktor =  P2D.Kp;
T1_Reaktor =  P2D.Tp1;
T2_Reaktor =  P2D.Tp2;
Tt_Reaktor =  P2D.Td;
G_Reaktor  =  tf([Kp_Reaktor],[T2_Reaktor T1_Reaktor 1]) %Totzeit in der Simu nicht vergessen

%%
%----- Dadurch Mantel schon PT2 Glied ist, funktioniert dieser Ansatz nicht mehr --------
%{
%----Zwei PT2 Glieder----
Kp_Mantel = P1.KP;
Kp_Zelle  = P2D.Kp/P1.Kp;
Kp_Total  = Kp_Mantel*Kp_Zelle ;

T1_Mantel  =P2D.Tp1;

T2_Zelle   =P2D.Tp2;
Tt_Zelle   =P2D.Td;
   
G_Mantel = tf([Kp_Mantel],[T1_Mantel 1])
G_Zelle  = tf([Kp_Zelle],[T2_Zelle 1])

%%
%----------------------------------------
%Simulation mit 2 PT1 Glieder
%}
G_Try=G_Reaktor/G_Mantel
G_Z_Mantel=c2d(G_Mantel,0.5)
G_Z_Zelle=c2d(G_Try,0.5)

sim('ColdPT1_Glieder');
figure('Name','Zelle');
plot(T_M.time, T_M.signals.values,'b', T_Z.time, T_Z.signals.values, 'r')

%Differenz von Simulation und Real
T_dZelle=[23140:1];
T_dMantel=[23140:1];
for x=1:fileende
    T_dZelle(x,1)=T_Zelle0(x,1)-T_Z.signals.values(x,1);
end
for x=1:fileende
    T_dMantel(x,1)=T_Mantel0(x,1)-T_M.signals.values(x,1);
end
figure('Name','Differenz T_Real zu T_Simulation');
plot(t,T_dMantel,'b', t,T_dZelle, 'r')

%%
%Simulation Reaktortemperatorverlauf mit PT2 Glied 
G_Z_Reaktor=c2d(G_Reaktor,0.5) 
sim('ColdPT2');
figure('Name','Reaktor');
plot(T_Reaktor.time, T_Reaktor.signals.values)

%Differenz von Simulation und Real
T_Diff=[23140:1];
for x=1:fileende
    T_Diff(x,1)=T_Zelle0(x,1)-T_Reaktor.signals.values(x,1);
end
figure('Name','Differenz T_Real zu T_Simulation');
plot(t,T_Diff)
